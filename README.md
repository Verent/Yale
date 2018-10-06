# Yale (.NET Standard 2.0)

Yale is yet another expression parser and evaluator library for .Net

Like other expression frameworks Yale evaluates string expressions like `sqrt(a^2 + b^2)` and  `name() = "Maria"` at runtime. In Yale, these expressions are compiled to the [Common Intermediate Language](https://en.wikipedia.org/wiki/Common_Intermediate_Language). This results in a blazing fast performance of expression evaluation after compilation. The primary design objective of Yale is to make it as intuitive and easy to use as possible.

Yale is based on [Flee](https://github.com/mparlak/Flee) with the intent to modernize and simplify the source code and usage. 

## What is the state of Yale?
Yale is in alpha and breaking changes should be expected. However most of the current work is focused on stabilization and bug-fixes. It should be safe for small applications and demos. Any bugs reported will be prioritized and feedback would be appreciated.

For more information read the [wiki](../../wiki)

## License
 
The MIT License (MIT)

Copyright (c) 2015 Chris Kibble

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
