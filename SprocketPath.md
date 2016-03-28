# SprocketPath #

A "Sprocket Path" refers to the portion of the URL after the base application path but before the querystring. It does not include leading or trailing slashes. The reason for this concept is that often you will develop your application in a different application path than where you deploy it.

Naturally you want internal URLs that should point to the root of your website to always go to the correct place, regardless of whether it is your development environment or your production environment.

For example your local application might be `http://localhost/dev/mywebsite/`, but your live website might be `http://www.mywebsite.com`.

In this case, the SprocketPath for the URL `http://localhost/dev/mywebsite/contact/thanks/?error=bad` would be `contact/thanks`

Throughout your development with Sprocket, practically all instances where you should specify a path will be in reference to the SprocketPath. The full address will usually be calculated on your behalf where relevant.