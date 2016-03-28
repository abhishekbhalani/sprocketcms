# SprocketScript #

SprocketScript is a simple text-based scripting language used by the !CMS module(s) for adding flexibility and extensibility to templates and content. It is also worth mentioning that the language has been designed with the idea in mind that anyone who wants to develop extensions for Sprocket can easily add new instructions and expressions to the language to support their own classes and functionality.

SprocketScript is completely optional, and only a single simple feature is required for making templates inherit from other templates. The more SprocketScript you use though, the more you can get out of the templating system, so it's worth learning at least the basics.

## The Basics ##

Anywhere in your templates and page content, you can embed SprocketScript syntax to cause the system to generate the content on the fly for your web page. SprocketScript is placed in blocks in your html code, starting with `{?` and ending with `}`. For example:

```
<p>Today's date is {? show currentdate }.</p>
```

The first and most common script instruction is the "show" instruction. For example:

`<html><head><title>{? show "The Page Title" }</title></head></html>`

would produce a page with the following html:

`<html><head><title>The Page Title</title></head></html>`

Naturally this isn't very useful by itself. Why not just write that to start with? This is where expressions come in. An expression is basically a reference to some value that we won't know until the script runs. An expression can sometimes be simple e.g. `{? show currentdate }` would render the current date in place of that bit of script. In this case "currentdate" is the expression. A slightly more complex expression would be `{? show 5 + 4 > 10 }` which basically means if 5 + 4 is greater than 10, the output is "true" otherwise it's "false". You'll see the usefulness of this feature later.

[do](to.md)

## QUICK REFERENCE ##

### Instruction / Syntax Examples: ###

```
~ this is a code comment for you to use wherever you like ~
section "[section name]" [instruction(s)...] end
show [expression]
if [expression] [instruction(s)...] end
if [expression] [instruction(s)...] else [instruction(s)...] end
if [expression] [instruction(s)...] elseif [expression] [instruction(s)] else [instruction(s)...] end
while [expression is true]
	[instruction(s)...]
loop
list each thingy in [expression]
	show thingy
	show thingy:property_of_thingy
loop
set somevariable to [expression]
show somevariable
```

"section" and "if" statements require a trailing "end" statement to indicate where their instruction list ends.

**Shorthand:**
  * `show` can be written as a `?` symbol. e.g. `{? show "hello" }` is the same as `{??"hello"}`
  * `section` can be written as an `@` symbol e.g. `{? section "main content" }` can be written as `{?@"main content"}`
  * `end` can be written as a `;` symbol e.g. `{? section "main content" end}` can be written as `{?@"main content";}`

### Expression Syntax ###

**Standard mathematical expressions:**
  * `+` add `{? if 2 + 2 > 10 show "impossible" else show "ok"; }`
  * `-` subtract `{? if 10 + 10 <= 5 show "impossible" else show "ok"; }`
  * `*` multiply `{? if 2 * 2 > 10 show "impossible" else show "ok"; }`
  * `/` divide `{? if 4 / 2 >= 10 show "impossible" else show "ok"; }`

**True/false (logical) expressions:**
  * `=` equal to e.g. `{?? 10 = 10 }` -> "True"
  * `>` greater than e.g. `{?? 5 > 6 }` -> "False"
  * `<` less than e.g. `{?? 5 < 6 }` -> "True"
  * `>=` greater than or equal to e.g. `{?? 5 >= 5 }` -> "True"
  * `<=` less than or equal to e.g. `{?? 10 <= 9 }` -> "False"
  * `<>` not equal to e.g. `{?? 5 <> 10 }` -> "True"
  * `not` - a special operator that makes the trailing value equal to everything other than itself. e.g. `{? if 5 is not 10 show...}`

**Shorthand (alternate syntax):**
  * `=` can be written as `is` e.g. `{? if 5 < 6 show "this always shows"; }`
  * `<>` can be written as `!=`, `isnt`, `isn't`, `is not` and `= not` e.g. `{? if 5 is   not 10 show "this always shows"; }`

## Special Expressions ##

Because Sprocket is designed to make it simple for developers to add their own expressions and instructions, there are a lot of different expressions that have been developed so far for returning various bits of information back to the scripting language. Some special instructions require an argument list in order to customise how the information is extracted. An argument list follows the expression keyword and contains one or more expressions separated by commas inside a set of brackets. For example:

```
show path(0) ~ shows the first bit of the SprocketPath ~
show lowercase(currentdate) ~ shows today's date in lower case ~
show htmlencode(username) ~ html encodes the currently-logged-in user's username ~
show page("home"):path ~ shows the path of the page identified in the definitions.xml file with Code="home" ~
show page("home"):path:length ~ shows the length of the path of the page identified in the definitions.xml file with Code="home" ~
```
