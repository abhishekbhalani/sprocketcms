# Basic Website Construction #

The current version of Sprocket supports a simple yet versatile page/template model which is defined by a single definitions file, one or more HTML templates and any number of optional XML files to hold content.

A future version of Sprocket will move this functionality to a database to make it easier to support large amounts of content, but when this happens, the current method will be preserved via an import/export system to copy the pages between the database and the XML file(s).

## 1. The Definitions File ##

Your website should have a base-level folder called "resources". This is the standard place to put all of your website assets such as html files, xml files, images, etc. Place a new XML file called `definitions.xml` in the resources folder.

This XML file has two main sections; Pages and Templates. Here is a sample definitions file:

```
<?xml version="1.0" encoding="utf-8" ?>
<Definitions>
	<Pages>
		<Page Path="testpage" Code="test_page" Template="test_template" HandleSubPaths="true" />
	</Pages>
	<Templates>
		<Template Name="test_template" File="resources/templates/html/test.htm" />
	</Templates>
</Definitions>
```

### Pages ###

A page is defined as a block of content that might be requestable directly via a URL, or embedded as a section in another page. This is useful because it allows you to delegate sections of content that appear in multiple locations to just one area, making your website easy to maintain.

**Page** entries have multiple attributes you can use and all of them are optional. The attributes are:

  * **Path**: A SprocketPath referring to the URL that should request this page
  * **Code**: An identifier word that you can use to refer to this page entry from other locations
  * **Template**: A reference to the name of the template that should be used to render this page
  * **ContentType**: The default is "text/html" if you leave it out. If this page entry is being used for rendering javascript, xml, or some other content type, set this attribute accordingly.
  * **HandleSubPaths**: Specifies that this page should render for all URLs that are descendants of the specified **Path** attribute.

### Templates ###

A template refers to an HTML page that contains some amount of HTML and/or inline SprocketScript code that renders output to be displayed for a particular page.

Templates are inheritable. That is, if you define certain areas in one template, you can define another template that uses the first template as the "master" template and overrides the areas you specify in that template with HTML of your choice.

The most basic definition of a Template simply involves naming the template and specifying some HTML for it to use.

```
<Template Name="test_template">
	This is the text for the template.
</Template>
```

You can also use a CDATA section:

```
<Template Name="test_template">
	<![CDATA[This is the <strong>text</strong> for the template.]]>
</Template>
```

Instead of typing in the HTML directly, you can specify which HTML file the template should use. Note that the file reference is a SprocketPath:

```
<Template Name="test_template" File="resources/html/template.htm" />
```

You may want to define a master template that, for example, defines a header, footer and sidebar. After doing this, you might want some child templates that reference the master template but specify the layout of the main content area. To do this, first define a SectionName in the master template using SprocketScript syntax. Then in the child template definition, specify (a) the master template and (b) the section(s) you want to override with values defined by the child template:

```
<Template Name="master_template">
	<![CDATA[
	<html>
	<head>
	<title>My Website - {? section "Title" }Home{? end }</title>
	</head>
	<body>
		{? section "ContentArea" }
		This is what appears if you only use the master template.
		Note that it appears inside an overridable section and thus
		will not appear if a child template specifies that something
		alternate should appear here.
		{? end }

	<div>This is the footer</div>
	</body>
	</html>
	]]>
</Template>

<Template Name="child_template" Master="master_template">
	<Replace Section="ContentArea" File="resources/html/somefile.htm" />
	<Replace Section="Title">
		The SubPage!
	</Replace>
</Template>
```