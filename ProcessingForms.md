# Quick How-To Guide for Processing Forms in Sprocket #

Because Sprocket is a significant sidestep from the traditional ASP.Net Page framework, processing of forms must be approached a little differently. Below I will give two different methods for processing form posts. _The Custom Way_ can be used if you have a deeper understanding of Sprocket and need to process things differently to normal. _The Proper Way_, though, uses a bunch of shortcuts designed to make your life easier when processing forms using Sprocket.

## Processing Forms the Proper Way ##

If you haven't already, get an overview of [basic website construction](BasicWebsiteConstruction.md) and SprocketScript.

The first thing you want to do is set up a page with a form on it and various fields. This method assumes that you're using the page/template system described in BasicWebsiteConstruction. The steps to having a fully-working submittable/processable form are generally:

  1. build the form HTML
  1. pre-populate the form values with either
    1. default values (e.g. for a new record) or
    1. pre-existing values (e.g. from an existing record)
  1. handle form submission, including:
    * saving the form values if they were all valid, then redirecting to another page
    * displaying the form page again if there were errors
  1. display error messages on the form if the form was submitted with any invalid field values
  1. display error styles if there are form field validation errors

### 1. Build the Form HTML ###

Make sure you have a page with a specified code (`<Page Path="contact" Code="my-contact-form" />`). Build your form, being sure to set the form action to the whatever page address is going to process the form. For the purposes of this documentation, point the form action at the same page that is serving the form:
```
<form method="post" action="{? show basepath show page:path }/">
...
</form>
```

### 2. Prepare the Form Field Values ###

How do we know which values the form should be displaying, and how do we get the values into the form HTML?

In your website module that you've set up for your own website processing functionality (if you're not sure, just use the code below), you need to interrupt the page processing right before the page is served so that you can prepare the default form values. This is done using the static method AddPagePreprocessor of the module ContentManager. The first argument is the page code for your form page (Remember `<Page Code="my-contact-form"...`?). The second is a pointer to the method that will handle the form preparation.
```
using Sprocket;
using Sprocket.Web;
using Sprocket.Web.CMS.Content;

[ModuleTitle("My Module")]
[ModuleDescription("This module does custom processing for my website")]
[ModuleDependency(typeof(WebEvents))]
class MyModule : ISprocketModule
{
	public void AttachEventHandlers(ModuleRegistry registry)
	{
		ContentManager.AddPagePreprocessor("my-contact-form", PrepareContactForm);
	}

	public void PrepareContactForm(PageEntry page)
	{
	}
}
```

Now whenever your contact page is about to be served, the method PrepareContactForm will be called first. Next let's prepare the actual form values. We use the static class FormValues, which is also used to set form errors when the form is submitted. The values you set here are only valid for the life of the current request and so are cleared every time a new page is requested. We call the method Set, which has the following arguments:

`FormValues.Set(field_name, error_message, field_value, is_field_invalid);`

```
public void PrepareContactForm(PageEntry page)
{
	// make sure we only prepare these values if the form hasn't been submitted yet.
	// this is like using ASP.Net's System.Web.UI.Page.IsPostback property.
	if(Request.Form.Count == 0)
	{
		FormValues.Set("name", "", "anonymous", false);
		FormValues.Set("phone", "", "don't wanna say", false);
	}
}
```

Now we need to go back to the HTML and use SprocketScript to get these values into the form. Above I've hard-coded certain values. What you'd normally do is only fill in the values if you're preloading from a database record or some other pre-existing data. As you'll see below, generic default values (generally used for forms that will become new records in the database) can be filled directly using SprocketScript.

```
Your Name: <input type="text" name="name" value="{?? formfield("name","some default value")}" />
Your Phone Number: <input type="text" name="phone" value="{?? formfield("phone","some default value")}" />
```

Notice the SprocketScript expression `formfield`. This takes two arguments. The first is the name of the field to check for a pre-existing value. The second is a value to use if no value has been previously specified. You have previously specified values for both fields in your PrepareContactForm method, but if you hadn't, the "some default value" arguments above would have ended up as the values of the input fields.

### 3. Handle Form Submission ###

Processing form submission in Sprocket is very easy. In your AttachEventHandlers method, you need to make a call to `WebEvents.AddFormProcessor(...)`, specifying a `FormPostAction` object as the argument. The `FormPostAction` constructor takes four arguments and a delegate method. The first four arguments are optional filters that determine conditions under which the delegate method is called. Specify `null` to indicate that the option is not to be considered as a condition of calling the delegate method. The arguments are:

  1. **PostToPath**: Represents the SprocketPath to which the form must be posted.
  1. **PostFromPath**: Represents the SprocketPath from which the form must be posted.
  1. **FieldName**: Represents the name of a field that must exist in the Request.Form collection
  1. **FieldValue**: Represents the value of the field specified by FieldName.
  1. **PostHandler**: A delegate method defined as:

```
void YourHandler()
{
  // handler code here
}
```

Considering the fact that any of the first four arguments can be specified as null, you can set simple conditions for the form post handler method to be called, such as the path posted to must be X, or a particular field Y must be submitted. Specific examples that have been used in the past:
```
void AttachEventsHandlers(ModuleRegistry registry)
{
    // handle a delete request (hidden fields have the details)
    WebEvents.AddFormProcessor(new FormPostHandler("product/delete",null,null,null,MyHandler1));

    // handle a specific button click on a page posting back to itself, making sure that
    // the page did indeed post back to itself. ensure that the button "SaveButton" was
    // clicked, but the value of the button is irrelevant.
    WebEvents.AddFormProcessor(new FormPostHandler("product/edit","product/edit","SaveButton",null,MyHandler2));

    // have a generic form post handler path that simply requires that the source form
    // specify, via a hidden field, what the form post action is.
    WebEvents.AddFormProcessor(new FormPostHandler(null,null,"FormPostAction","save_product",MyHandler3));
}
```

Note that forms are processed directly BEFORE the WebEvents.OnLoadRequestedPath event fires and thus, before any CMS page handling, etc. This provides a good opportunity to redirect to another page upon successful form processing if desired, or whatever else is relevant for the operation in question.

## Processing Forms the Custom Way ##

Let's say you have the following form somewhere on a page in your website:
```
<form method="post" action="/submit-my-form/">
	Your name: <input type="text" name="name" />
	<input type="submit" />
</form>
```

You should now make a generic module (or use one you've already created). Here is a basic module template:
```
using Sprocket;
using Sprocket.Web;

[ModuleDependency(typeof(WebEvents))]
[ModuleTitle("My Request Handler")]
[ModuleDescription("This is a basic module for handling requests")]
public class MyRequestHandler : ISprocketModule
{
	public void AttachEventHandlers(ModuleRegistry registry)
	{
		WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(WebEvents_OnLoadRequestedPath);
	}

	void WebEvents_OnLoadRequestedPath(HandleFlag handled)
	{
		switch (SprocketPath.Value)
		{
			case "submit-my-form":
				ProcessYourForm();
				break;

			default:
				return;
		}
		handled.Set(); // ensure that Sprocket considers the request handled
	}
}
```
