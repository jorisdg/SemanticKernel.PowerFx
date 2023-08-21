# SemanticKernel.PowerFx
This repo is a proof-of-concept for adding low-code native functions to Semantic Kernel using Power Fx.

## Semantic Kernel
[Semantic Kernel is an open source project](https://github.com/Microsoft/semantic-kernel) from Microsoft. It is an SDK that integrates Large Language Models (LLMs) like OpenAI, Azure OpenAI, and Hugging Face with conventional programming languages like C#, Python, and Java. Semantic Kernel achieves this by allowing you to define plugins that can be chained together in just a few lines of code.

## Orchestration
What makes Semantic Kernel (SK) most interesting is its orchestration and [planning capabilities](https://learn.microsoft.com/semantic-kernel/ai-orchestration/planner?tabs=Csharp). Given a set of plugins, SK is able to determine a list of plugins to use based on the _ask_ from a user. It is able to chain these together and pair inputs and outputs between them to come to an answer.

## Plugin functions
Beyond supporting the recently [standardized plugins for OpenAI](https://platform.openai.com/docs/plugins/getting-started/), Semantic Kernel (SK) allows you to create your own functions as plugins. SK supports **semantic** functions, which are basically prompts for an LLM with defined inputs and outputs, as well as **native** functions. [Native functions](https://learn.microsoft.com/semantic-kernel/ai-orchestration/native-functions?tabs=Csharp) are functions that are defined ahead of time in the host of the SK. These functions can be written in C# or Python.

## Low-Code Power Fx Functions
So the question is - why shouldn't we be able to define functions with low code as well? Suppose a low-code environment has the ability to expose its functions (predefined ones and/or user defined ones) to the Semantic Kernel, it would allow for users of that app to use natural language to execute commands. [Power Fx is another open source Microsoft project](https://github.com/Microsoft/Power-Fx) that provides a low code functional language with an Excel-like syntax.

## Disclaimer
The code here is provided as-is. It is my own personal project and just a proof-of-concept. This is not an officially sanctioned or supported project.
