# How to Write Sprocket Modules #

## The Basics ##

Sprocket modules are surprisingly easy to write. How complex they get really depends on you and how tricky you're trying to be. Anyone developing a website that has to process at least one form should probably be writing a simple module. (see ProcessingForms for more info)

Why write a module? Well, Sprocket only provides so many ways to do things out of the box. Inevitably you're going to want to make your website do something that is specific to that website. This means you're going to want to respond to specific types of requests, serve things up in weird formats, or really whatever you can think of.

So the first thing to do is to define your module. All you have to do is implement `ISprocketModule` and apply a couple of attributes to describe it to Sprocket's module registry.

```
using Sprocket;
using Sprocket.Web;

namespace MyProject
{
	[ModuleTitle("My Module")]
	[ModuleDescription("This is my module")]
	public class MyModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
		}
	}
}
```

Voila! You've just written the most basic Sprocket module possible. Make sure the class is referenced by your web project, along with all the other Sprocket projects, and your module will be automatically registered. To check it out, go to http://wherever-sprocket-runs-from/sysinfo/ and you'll see your module listed, plus you'll see it included in the module dependency diagram. It'll be all by itself at the top of the diagram, because so far it isn't dependent on anything else.

So this is all great, but we should probably make the module do something. To learn how to use it to process forms, go check out ProcessingForms. Here though, we'll learn how to make it serve up a bit of custom HTML.

**FIRST read about SprocketPath! [SprocketPaths](SprocketPath.md) are an integral concept in Sprocket and I'm about to make reference to them.**

The first thing to do is to hook into the appropriate event handler. We're going to hook into WebEvents.OnLoadRequestedPath. This event basically says "ok i've done some pre-processing, i'm kinda hoping there is some content to serve now". It passes you a single object ("Handled") which you can use to specify whether or not you've taken care of this request. If you don't, Sprocket will continue on and quite possibly end up at a 404 page. Inside your event handler, you should do something, like for example writing something to the HTTP output stream, then set the Handled flag. Like so:

```
using Sprocket;
using Sprocket.Web;

namespace MyProject
{
	[ModuleTitle("My Module")]
	[ModuleDescription("This is my module")]
	public class MyModule : ISprocketModule
	{
		public void AttachEventHandlers(ModuleRegistry registry)
		{
			WebEvents.Instance.OnLoadRequestedPath += new WebEvents.RequestedPathEventHandler(Instance_OnLoadRequestedPath);
		}

		void Instance_OnLoadRequestedPath(HandleFlag handled)
		{
			// make sure the SprocketPath is "hello" so that we only respond to the right request
			if (SprocketPath.Value != "hello")
				return;

			HttpContext.Current.Response.Write("<h1>Hello, world!</h1>");
			handled.Set();
		}
	}
}
```

If you've installed Sprocket to a local virtual folder accessible via http://localhost/myproject/, you'll find you can now get your module to serve up a page by calling http://localhost/myproject/hello/

Easy! Your first module is complete.

#### Important Stuff to Remember ####

| Just a quick bit of info for those not in the know about how ASP.Net works under the hood. |
|:-------------------------------------------------------------------------------------------|

Each time someone makes a request from your website, a new worker process (or an existing one already in the pool) is created and allocated exclusively to handle that request. That process has no job other than seeing this one request through to completion. When it is done, it is thrown back in the pool where it waits for another request, or is eventually discarded for memory reasons or because somebody invalidated it by, for example, changing the Web.Config file.

> Your module is loaded into memory when the worker process is created, and it stays alive this whole time! This means any member variables you create and don't kill at the end of the request (see WebEvents.OnEndHttpRequest) are preserved for the next person who ends up with this worker process. So be sure to clean up when the request is finished.

Also, all worker processes share access to static variables in memory, so be **very** careful about storing things in static variables if you don't know much about thread safety. Otherwise you are going to run into problems. If you need to have some global stuff accessible during the request, use the CurrentRequest class (see Sprocket.Web.CurrentRequest). It's basically a little repository you can stash whatever you want in and have it automatically cleared at the end of the request.