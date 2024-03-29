using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Sprocket;
using Sprocket.Data;
using Sprocket.Security;
using Sprocket.Web;
using Sprocket.Web.Controls;
using Sprocket.Utility;

namespace Sprocket.Web.CMS.Security
{
	[AjaxMethodHandler()]
	partial class WebSecurity
	{
		void OnAjaxRequestAuthenticationCheck(System.Reflection.MethodInfo source, Result result)
		{
			if (!CurrentUser.Enabled)
			{
				result.SetFailed("Ajax method called failed because your account has been disabled.");
				return;
			}

			Attribute[] roleAttr = Attribute.GetCustomAttributes(source, typeof(RequiresRoleAttribute));
			Attribute[] permAttr = Attribute.GetCustomAttributes(source, typeof(RequiresPermissionAttribute));
			for (int i = 0; i < roleAttr.Length; i++)
				if (!CurrentUser.HasRole(((RequiresRoleAttribute)roleAttr[i]).RoleCode))
				{
					result.SetFailed("Ajax method call failed because you do not have one or more required roles.");
					return;
				}
			for (int i = 0; i < permAttr.Length; i++)
				if (!CurrentUser.HasPermission(((RequiresPermissionAttribute)permAttr[i]).PermissionTypeCode))
				{
					result.SetFailed("Ajax method call failed because you do not have one or more required permissions.");
					return;
				}
		}

		#region User Editing and Filtering
		public class UserFilterResults : IJSONEncoder
		{
			int matches;
			SecurityProvider.User[] users;

			public UserFilterResults(int matches, SecurityProvider.User[] users)
			{
				this.matches = matches;
				this.users = users;
			}

			public void WriteJSON(StringWriter writer)
			{
				writer.Write("{");
				JSON.EncodeNameValuePair(writer, "Matches", matches);
				writer.Write(",");
				JSON.EncodeString(writer, "Users");
				writer.Write(":[");
				int c = 0;
				foreach (SecurityProvider.User user in users)
				{
					if (c++ > 0) writer.Write(",");
					JSON.EncodeCustomObject(writer,
						new KeyValuePair<string, object>("Name", (user.FirstName + " " + user.Surname).Trim()),
						new KeyValuePair<string, object>("Username", user.Username),
						new KeyValuePair<string, object>("UserID", user.UserID)
					);
				}
				writer.Write("]}");
			}
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		public UserFilterResults FilterUsers(string username, string firstname, string surname, string email, int max)
		{
			int total;
			SecurityProvider.User[] users = SecurityProvider.User.BasicSearch(username, firstname, surname, email, max, CurrentUser.UserID, out total, null);
			UserFilterResults results = new UserFilterResults(total, users);
			return results;
		}

		public delegate void UserEditFormLayoutHandler(Guid? userID, bool showErrors, AjaxForm form);
		public event UserEditFormLayoutHandler OnUserEditFormLayout;

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		public AjaxForm GetUserEditForm(Guid? userID)
		{
			/* business rules:
			 * people with user administration access can only see user accounts that have a subset of the logged-in user's own roles/permissions
			 * user accounts containing roles or permissions that are not possessed by this user can NOT be altered by the current user
			 * the current user can only assign roles or permissions to other users if he/she has that role or permission
			 */
			string fErr = "function(value){{if(value.length==0) return 'Please enter a {0}'; return null;}}";
			string pErr = userID != null ? null : string.Format(fErr, "password");
			string username = null, firstname = null, surname = null, email = null, blockheading = null;
			bool enabled = true, locked = false;
			if (userID != null)
			{
				SecurityProvider.User user = SecurityProvider.User.Load(userID.Value);
				if (!CurrentUser.CanModifyUser(user))
					throw new AjaxException("You don't have access to modify that user.");
				username = user.Username;
				firstname = user.FirstName;
				surname = user.Surname;
				email = user.Email;
				enabled = user.Enabled;
				locked = user.Locked;
			}
			blockheading = "User Details";
			AjaxForm form = new AjaxForm("UserEditForm");
			if (userID != null) form.RecordID = userID.Value;
			AjaxFormFieldBlock block = new AjaxFormFieldBlock("MainUserFields", blockheading);
			block.Add(new AjaxFormInputField("Username", "Username", 50, locked, null, "width:150px;", username, null, string.Format(fErr, "username"), true, 0));
			block.Add(new AjaxFormInputField("Password", "Password", 50, false, null, "width:150px;", null, null, pErr, true, 1));
			block.Add(new AjaxFormInputField("First Name", "FirstName", 50, false, null, "width:150px;", firstname, null, null, true, 2));
			block.Add(new AjaxFormInputField("Surname", "Surname", 50, false, null, "width:150px;", surname, null, null, true, 3));
			block.Add(new AjaxFormInputField("Email", "Email", 100, false, null, "width:150px;", email, null, string.Format(fErr, "valid email address"), true, 4));
			block.Add(new AjaxFormCheckboxField("User account is enabled", "Enabled", enabled, locked, null, null, false, 5));
			block.Rank = -10000;
			form.FieldBlocks.Add(block);

			if (!locked && username != CurrentUser.Username)
			{
				block = new AjaxFormFieldBlock("Roles", "Assigned Roles");
				block.Rank = 998;
				IDbCommand cmd = Database.Main.CreateCommand("ListRolePermissionStates", CommandType.StoredProcedure);
				Database.Main.AddParameter(cmd, "@UserID", userID);
				DataSet ds = Database.Main.GetDataSet(cmd);
				int c = 0;
				foreach (DataRow row in ds.Tables[0].Rows)
				{
					// check that the current user has access to assign the specified permission/role
					if (!CurrentUser.HasRole(row["RoleCode"].ToString())) continue;
					block.Add(new AjaxFormCheckboxField(row["Name"].ToString(), row["RoleCode"].ToString(),
						(bool)row["HasRole"], false, null, null, false, c++));
				}
				if (c > 0) form.FieldBlocks.Add(block);

				block = new AjaxFormFieldBlock("Permissions", "Specific Assigned Permissions");
				block.Rank = 999;
				c = 0;
				foreach (DataRow row in ds.Tables[1].Rows)
				{
					// check that the current user has access to assign the specified permission/role
					if (!CurrentUser.HasPermission(row["PermissionTypeCode"].ToString())) continue;
					block.Add(new AjaxFormCheckboxField(row["Description"].ToString(), row["PermissionTypeCode"].ToString(),
						(bool)row["HasPermission"], false, null, null, false, c++));
				}
				if (c > 0) form.FieldBlocks.Add(block);
			}
			block = new AjaxFormFieldBlock("SubmitButtons", null);
			AjaxFormButtonGroup buttons = new AjaxFormButtonGroup();
			block.Rank = 10000;
			buttons.AddSubmitButton(null, "Save", "SecurityInterface.OnUserSaved", null);
			if (userID != null)
			{
				if (!locked) buttons.AddButton(null, "Delete", "SecurityInterface.DeleteUser('" + userID.ToString() + "')");
				//buttons.AddButton(null, "Send Password", "SecurityInterface.SendPassword('" + userID.ToString() + "')");
				buttons.AddButton(null, "Cancel", "SecurityInterface.CancelUserEdit()");
			}
			block.Add(buttons);
			form.FieldBlocks.Add(block);

			if (OnUserEditFormLayout != null)
				OnUserEditFormLayout(userID, false, form);

			return form;
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		public Result DeleteUser(Guid userID)
		{
			SecurityProvider.User user = SecurityProvider.User.Load(userID);
			if (!CurrentUser.CanModifyUser(user))
				return new Result("You don't have permission to modify this user.");
			if (user.Locked)
				return new Result("This user cannot be deleted.");
			return ((SecurityProvider)Core.Instance["SecurityProvider"]).DeleteUser(user);
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		public Result SendPasswordReminder(Guid userID)
		{
			SecurityProvider.User user = SecurityProvider.User.Load(userID);
			try
			{
				user.SendPasswordReminder(WebUtility.CacheTextFile("resources/passwordreminder.email.txt"));
				return new Result();
			}
			catch(Exception ex)
			{
				return new Result(ex.Message);
			}
		}
		#endregion

		#region Role Editing
		public class RoleItem : IJSONEncoder
		{
			private string name;
			private Guid id;

			public RoleItem(string name, Guid id)
			{
				this.name = name;
				this.id = id;
			}

			public void WriteJSON(StringWriter writer)
			{
				JSON.EncodeCustomObject(writer,
					new KeyValuePair<string, object>("Name", name),
					new KeyValuePair<string, object>("RoleID", id.ToString())
				);
			}
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		[RequiresPermission("ROLEADMINISTRATOR")]
		public List<RoleItem> GetAccessibleRoles()
		{
			IDbCommand cmd = Database.Main.CreateCommand("ListAccessibleRoles", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@UserID", CurrentUser.UserID);
			DataSet ds = Database.Main.GetDataSet(cmd);
			List<RoleItem> roles = new List<RoleItem>();
			foreach (DataRow row in ds.Tables[0].Rows)
				if (!(bool)row["Locked"])
					roles.Add(new RoleItem(row["Name"].ToString(), (Guid)row["RoleID"]));
			return roles;
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		[RequiresPermission("ROLEADMINISTRATOR")]
		public AjaxForm GetRoleEditForm(Guid? roleID)
		{
			SecurityProvider.Role role;
			if (roleID == null)
				role = new SecurityProvider.Role();
			else
			{
				role = SecurityProvider.Role.Load(roleID.Value);
				if (role == null)
					throw new AjaxException("The requested role does not exist in the database.");
				if (role.Locked)
					throw new AjaxException("This is a system role and cannot be modified.");
			}

			AjaxForm form = new AjaxForm("RoleEditForm");
			form.RecordID = roleID;

			AjaxFormFieldBlock block = new AjaxFormFieldBlock("RoleDetails", "Role Details");
			block.Add(new AjaxFormInputField("Role Name",
				"Name", 100, role.Locked, null, null, role.Name, null,
				"function(value){{if(value.length==0) return 'A name is required'; return null;}}",
				true, 0));
			block.Add(new AjaxFormCheckboxField("Role is enabled", "Enabled", role.Enabled, role.Locked, null, null, false, 1));
			block.Rank = 0;
			form.FieldBlocks.Add(block);

			List<Guid> roleDescendents = new List<Guid>();
			IDbCommand cmd = Database.Main.CreateCommand("ListDescendentRoles", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@RoleID", role.RoleID);
			DataSet ds = Database.Main.GetDataSet(cmd);
			foreach (DataRow row in ds.Tables[0].Rows)
				roleDescendents.Add((Guid)row["RoleID"]);

			cmd = Database.Main.CreateCommand("ListRoleToRoleAssignmentStates", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@RoleID", role.RoleID);
			ds = Database.Main.GetDataSet(cmd);

			block = new AjaxFormFieldBlock("Roles", "Roles that this role should adopt");
			block.Rank = 1;
			int c = 0;
			foreach (DataRow row in ds.Tables[0].Rows)
				if (CurrentUser.HasPermission(row["RoleCode"].ToString()) && !roleDescendents.Contains((Guid)row["RoleID"]))
					block.Add(new AjaxFormCheckboxField(
						row["Name"].ToString(), row["RoleCode"].ToString(),
						(bool)row["Inherited"], role.Locked, null, null, false, c++));
			if (block.Count > 0)
				form.FieldBlocks.Add(block);

			cmd = Database.Main.CreateCommand("ListPermissionValuesForRole", CommandType.StoredProcedure);
			Database.Main.AddParameter(cmd, "@RoleID", role.RoleID);
			Database.Main.AddParameter(cmd, "@ShowAllPermissions", true);
			ds = Database.Main.GetDataSet(cmd);

			block = new AjaxFormFieldBlock("Permissions", "Permission Settings");
			c  = 0;
			foreach (DataRow row in ds.Tables[0].Rows)
				if (CurrentUser.HasPermission(row["PermissionTypeCode"].ToString()))
					block.Add(new AjaxFormCheckboxField(
						row["Description"].ToString(), row["PermissionTypeCode"].ToString(),
						row["Value"] == DBNull.Value ? false : (bool)row["Value"], role.Locked, null, null, false, c++));

			AjaxFormButtonGroup buttons = new AjaxFormButtonGroup();
			block.Rank = 2;
			buttons.Rank = 10000;
			buttons.AddSubmitButton(null, "Save", "SecurityInterface.OnRoleSaved", null);
			if (roleID != null)
				if (!role.Locked) buttons.AddButton(null, "Delete", "SecurityInterface.DeleteRole('" + roleID.ToString() + "')");
			buttons.AddButton(null, "Cancel", "$('security-permissionlist').innerHTML = '';");
			block.Add(buttons);

			if (block.Count > 0)
				form.FieldBlocks.Add(block);

			return form;
		}

		[AjaxMethod(RequiresAuthentication = true)]
		[RequiresPermission("USERADMINISTRATOR")]
		[RequiresPermission("ROLEADMINISTRATOR")]
		public Result DeleteRole(Guid roleID)
		{
			SecurityProvider.Role role = SecurityProvider.Role.Load(roleID);
			if (!CurrentUser.HasRole(role.RoleCode))
				return new Result("You don't have permission to modify this role.");
			if (role.Locked)
				return new Result("This role cannot be deleted.");
			return ((SecurityProvider)Core.Instance["SecurityProvider"]).DeleteRole(role);
		}
		#endregion
	}
}
