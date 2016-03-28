# Quick Installation Guide #

The following steps will get you up and running with Sprocket in no time at all.

### Step 1 ###
Make sure ASP.Net 2.0 and SQL Server 2000 (or better) is installed. You'll need to go and set up a SQL Server database of course and organise login details so you can fill in the connection string for Sprocket so it knows how to connect to the database.

### Step 2 ###
Sprocket requires its root folder to be a proper IIS website or virtual directory. It won't work in an unconfigured subdirectory. Just go into IIS and if you're running WinXP Pro, create a new virtual directory or website, and point it to wherever you extracted Sprocket to. Make sure you set up full permissions so that Sprocket can read and write files and folders. Don't turn on directory browsing because Sprocket requests usually refer to locations that don't actually physically exist.

### Step 3 ###
An IIS wildcard mapping is required for ASP.Net requests. Basically, you must open up the website or virtual directory properties, click the "configuration" button and add a new mapping. For Windows Server 2003, you add a wildcard mapping, which has its own section at the bottom of the dialog box. IIS 6 (WinXP Pro) is the same, except you have to add it in the same place as all the other mappings. The file type is just "`.*`". Navigate to `C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727` (or wherever your 2.0 framework is installed) and select the `aspnet_isapi.dll` file to handle the request. Make sure the "check that file exists" checkbox is unchecked.

### Step 4 ###
Modify the connection string in the Web.config file so that it points at your SQL Server database. Feel free to make any other changes that blow your skirt up.

### Step 5 ###
If you've set this up from the subversion repository, remember to build the solution first. If you've downloaded the release, then it has already been built for you. Point your browser at http://[where-you-installed-sprocket]/$dbsetup in order for basic stored procedures and whatnot to be created.

### Step 6 ###
Next point your browser at your the wherever you installed Sprocket (e.g. http://localhost/Sprocket/) and hey presto, you're away! If you follow the link to the admin area (which is incomplete), the default username is admin and the password is password.

### Addendum ###
The included sample website has a bit of extra info in, plus this wiki has a bunch of help. See BasicWebsiteConstruction for more info.

Got questions? Post them at http://groups.google.com/group/Sprocket-CMS-Discussion