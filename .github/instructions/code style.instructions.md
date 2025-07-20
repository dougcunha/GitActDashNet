---
applyTo: '**/*.cs'
---
Formatting and style standards for this C# project:

## For writing code:

- 4-space indentation.
- Always use a blank line between code blocks (methods, properties, constructors, etc).
- Always add a blank line before if, for, foreach, and return.
- Always add a blank line after opening or before closing braces.
- Use expression-bodied members and start a new line before =>
- When calling a method with more than 4 arguments, chop the arguments so that each argument is written on its own line, 
the opening parenthesis of the method call or lambda expression must be placed on a new line after the method name. 
Additionally, if an argument uses a lambda expression or a nested call with its own parentheses, apply the same rule recursively.
- Constant names in UPPER_CASE.
- Use var when the type is obvious.
- XMLDoc is mandatory for public classes and methods.
- Non-inheritable classes must be sealed.
- DRY: avoid code repetition.
- Write code always in english.
- Use modern collections and initializers ([] and [..] when possible).
- Add <inherited /> for inherited members in XMLDoc.
- Always put using before namespace declaration and sort them alphabetically.
- Use `nameof` operator instead of hardcoded strings for member names.
- Make anonymous function static when possible.
- Keep always one class per file.
- Use `string.Equals` with `StringComparison.OrdinalIgnoreCase` for case-insensitive comparisons.
- Use `StringBuilder` for string concatenation in loops or large concatenations.
- Use .ConfigureAwait(false) for async methods to avoid deadlocks except in Windows Forms applications.
- Use `CancellationToken` for long-running operations to allow cancellation.

## Formatting Rules

### 1. Braces and Expression-Bodied Members
- **Single-line expressions**: Use expression-bodied members instead of braces
- **Multi-line blocks**: Use traditional braces with proper indentation
- **Expression-bodied syntax**: Place the body on a new line with 4-space indentation

**Examples:**
```csharp
// ✅ Correct - Expression-bodied property
public int MyProperty 
    => 42;

// ✅ Correct - Expression-bodied method
public void MyMethod() 
    => Console.WriteLine("Hello, World!");

// ✅ Correct - Multi-line method with braces
public int Calculate()
{
    var result = SomeCalculation();
    ProcessResult(result);
    
    return result;
}
```

- **Conditional Statements Without Braces**: Do not use braces {} for single-line conditional statements. Braces should only be used if at least one of the instructions within the conditional block spans more than one line.

```csharp
// ✅ Correct - Single-line conditional without braces
if (condition)
    DoSomething();

// ✅ Correct - Multi-line conditional with braces
if (condition)
{
    DoSomething();
    LogAction();
}
```

### 2. Blank Lines
- **Method separation**: Use blank lines to separate logical sections
- **Before return, break and continue statements**: Always add one blank line before `return`, `break`, and `continue` statements
**Example:**
```csharp
// ✅ Correct - Blank line before return
public int Calculate()
{
    var intermediate = PerformCalculation();
    var processed = ProcessData(intermediate);
    
    return processed; // Blank line before return
}

// ✅ Correct - Blank line before break and continue
public void ProcessData()
{
    foreach (var org in organizations)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Operation was cancelled while fetching organization repositories");

            break; // Blank line before break
        }

        if (org == null)
        {
            logger.LogWarning("Organization is null, skipping");

            continue; // Blank line before continue
        }

        // Process organization
        logger.LogInformation("Processing organization: {OrgName}", org.Name);
    }
}
```

### 3. Method Parameters
- **4+ parameters**: Each parameter on its own line
- **Parameter formatting**: 
  - Opening parenthesis on new line
  - Each parameter indented 4 spaces on separate lines
  - Closing parenthesis on new line, aligned with opening

**Example:**
```csharp
// ✅ Correct - Multi-parameter method
public void MyMethod
(
    string firstParameter,
    int secondParameter,
    bool thirdParameter,
    DateTime fourthParameter
)
{
    // Method body
}

// ✅ Correct - Few parameters stay inline
public void SimpleMethod(string param1, int param2)
{
    // Method body
}
```

### 4. Comments and XML Documentation
- **Language**: Always write comments in English
- **Style**: Use clear, concise English descriptions

**Examples:**
```csharp
// ✅ Correct - English comments
// Calculate the total price including tax
public decimal CalculateTotal(decimal basePrice)
    => basePrice * 1.2m;

// ❌ Incorrect - Non-English comments
// Calcula o preço total incluindo impostos
```

### 5. Naming Conventions
- **Variables and parameters**: `camelCase`
- **Fields**: `_camelCase` starting with a _
- **Methods, properties, classes**: `PascalCase`  
- **Constants**: `UPPER_CASE` with underscores

**Examples:**
```csharp
// ✅ Correct naming
public class OrderProcessor
{
    private const int MAX_RETRY_COUNT = 3;
    private string _customerName;
    
    public string CustomerName { get; set; }
    
    public void ProcessOrder(int orderId, string productName)
    {
        var isValid = ValidateOrder(orderId);
        // ...
    }
}
```

### 6. Namespace Declarations
- **File-scoped namespaces**: Always use file-scoped namespace syntax
- **Using statements**: Place at top of file, ordered alphabetically
- **Implicit usings**: Leverage implicit usings for common namespaces

**Example:**
```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyProject.Services;

public class OrderService
{
    // Class implementation
}
```

### 7. Modern C# Features
- **Latest language features**: Always use preview C# features when appropriate
- **Collection expressions**: Use collection expressions for initializing collections

**Examples:**
```csharp
// ✅ Correct - Collection expressions
List<string> myList = 
[
    "item1",
    "item2", 
    "item3"
];

// ✅ Correct - Other modern features
string[] names = ["Alice", "Bob", "Charlie"];
Dictionary<string, int> scores = new()
{
    ["Alice"] = 100,
    ["Bob"] = 85
};
```

### 8. Method Call Formatting Rule
Multi-Parameter Method Calls with Object Initializers When calling a method that has 2 or more parameters and at least one parameter uses object initialization syntax (e.g., new ClassName { ... }), format the method call as follows:

1. Method name on its own line
2. Opening parenthesis on the next line
3. Each parameter on its own line with proper indentation
4. Object initializers properly formatted with braces on separate lines
5. Closing parenthesis on its own line, aligned with the method name

**Example:**
```csharp
// ✅ Correct - Multi-parameter with object initializer
var json = JsonSerializer.Serialize
(
    value,
    new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IndentSize = 2
    }
);

// ✅ Correct - Multi-parameter with multiple object initializers
var result = SomeMethod
(
    firstParam,
    new ConfigOptions
    {
        EnableFeature = true,
        Timeout = 5000
    },
    new ProcessingOptions
    {
        Parallel = true,
        MaxThreads = 4
    }
);

// ✅ Correct - Simple parameters stay inline
var simple = SomeMethod(param1, param2);

// ✅ Correct - Single object initializer can stay inline if short
var config = new Options { Name = "test" };
```

## For writing tests:

- Use xUnit, Shouldly and NSubstitute.
- Place ExcludeFromCodeCoverage attribute on all test classes.
- All test classes must be public sealed.
- Use AAA (Arrange, Act, Assert) pattern.
- Use Method_Condition_ExpectedResult naming convention for test methods.
- Declare all dependencies as readonly fields and initialize it inline.
- Declare string const as const and use UPPER_CASE for names
- Use raw string to declare multi-line strings
- Avoid using magic strings and numbers, declare them as constants.
- Never try to mock non virtual classes
- If you can't mock a class, use a real instance of it or extract it to an interface.
- Use verbatim strings for file paths and other strings that require escaping.

## For writing documentation:

- Use Markdown for documentation.
- Use proper headings and subheadings.
- Use bullet points and numbered lists for clarity.
- Use code blocks for code snippets.
- Use links for references and further reading.
- Keep documentation up to date with code changes.
- Write always in english.
- Keep a section dedicate to give credits to third-party libraries used in the project.



