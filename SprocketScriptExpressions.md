# Expressions Available in SprocketScript #
Below is a list of the various expressions currently available in SprocketScript. Please note that this may not be a complete list.

| **Expression Keyword** | **Description** | **Arguments** | **Properties** | **Example** |
|:-----------------------|:----------------|:--------------|:---------------|:------------|
| currentdate            | Returns the current date | -             | -              | {?? currentdate } |
| formatdate             | Formats the specified date according to a supplied .Net DateTimeFormat | Two arguments; the date to format and the format string to use | -              | {?? formatdate(currentdate, "MMMM d, yyyy h:mmtt") |
| howlongago             | Returns a descriptive string expressing the supplied date as an amount of time passed. e.g. A date value that is 57 minutes ago would return "Almost an hour ago" | One argument; a date/time value | -              | {?? howlongago(currentdate) } |
| htmlencode             | Formats the specified string as an html string, i.e. encoding < and > as &lt; and &gt;, etc. | One or more arguments containg strings to format | -              | {?? htmlencode("<p>hello</p>") } |
| urlencode              | Formats the expression as a safe URL string, e.g. turning "1 2" into "1%202" | One or more arguments containing expressions to encode | -              | {?? urlencode("bob & harry?") } |
| lowercase              | Returns the supplied arguments as lower-case strings | One or more arguments that should be formatted as lower case strings | -              | {?? lowercase("JOHNSON") } |
| uppercase              | Returns the supplied arguments as upper-case strings | One or more arguments that should be formatted as upper case strings | -              | {?? uppercase("JOHNSON") } |
| safehtmlencode         | Adjusts the supplied strings to remove potentially harmful HTML such as script tags and iframes. Other HTML is left intact to render as proper unencoded HTML. | One or more arguments specifying HTML code to adjust. | -              | {?? safehtmlencode(some\_potentially\_hostile\_html) } |