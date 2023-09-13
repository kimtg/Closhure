# The Closhure Programming Language

Closhure is a dialect of Lisp. It is designed to be an embedded language (minimal Lisp for .NET Framework). It uses Clojure-like syntax.

## Influenced by
[Clojure](https://clojure.org/), [Javelin](https://github.com/kimtg/Javelin), [Schemy](https://github.com/microsoft/schemy)

## Influenced
[Javelin](https://github.com/kimtg/Javelin)

## Key learnings
1. Lisp interpreter
2. Reflection
3. Multi-line input
4. Tail call optimization

## Target ##
.NET Framework

## Run ##
```
Usage: Closhure.exe [OPTION] [ARGS...]

Options:
    FILE  run a script.
    -h    print this screen.
    -r    run a REPL.
    -v    print version.
Operation:
    Binds *command-line-args* to a list of strings containing command line args that appear after FILE.
```

## Reference ##
```
Special forms:
 . and catch def defmacro do doseq finally fn if import let loop new or quasiquote quote recur reify set! try
Defined symbols:
 * *command-line-args* + - / < <= = == > >= apply eval filter fold gensym instance? list load-file load-string macroexpand map mod nil? not not= nth pr print println prn quot range read read-line read-string slurp spit str symbol type
Macros:
 defn dotimes when while
```

[Clojure Cheatsheet](https://clojure.org/api/cheatsheet)

## Examples ##
### Hello, World! ###
```
(println "Hello, World!")
```

### Whitespace ###
` `, `\t`, `\r`, `\n`, `,` are whitespaces.

### Comment ###
```
; end-of-line comment
```

### Reader syntax ###
```
' quote
` quasiquote
~ unquote
~@ unquote-splicing
```

### Data types ###
You can use all C#'s data types.

Literals:
```
> (map type '(3 3L 3.0 3e3 true false nil "string" #"regex" \a :a () []))
(System.Int32 System.Int64 System.Double System.Double System.Boolean System.Boolean nil System.String System.Text.RegularExpressions.Regex System.Char Closhure.Keyword System.Collections.Generic.List`1[[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]] System.Collections.ArrayList)
```
* Characters - preceded by a backslash: \c. \newline, \space, \tab, \formfeed, \backspace, and \return yield the corresponding characters. Unicode characters are represented with \uNNNN as in C#. Octals are represented with \oNNN.
* nil Means 'nothing/no-value'- represents C# null and tests logical false
* Booleans - true and false

### Special form ###
```
> (let (a 1, b 2) (+ a b)) ; , is whitespace. () and [] are interchangeable in special forms.
3
> (doseq (x '(1 2 3)) (print x))
123nil
; (try EXPR ... (catch CLASS VAR EXPR ...) ... (finally EXPR ...))
> (try (quot 1 0) (catch System.DivideByZeroException e 'c) (finally (println 'f)))
f
c
```

### Function ###

In a function, [lexical scoping](http://en.wikipedia.org/wiki/Lexical_scoping#Lexical_scoping) is used.

Functions implement IComparer<object> interface.

Callable
```
> (. * call)
1
```

IComparer<object>
```
> (def a (list 3 2 1))
(3 2 1)
> (. a sort a -)
nil
> a
(1 2 3)
```

```
> ((fn (x y) (+ x y)) 1 2)
3
> ((fn (x) (* x 2)) 3)
6
> (defn foo (x & more) (list x more)) ; variadic function
(fn (x & more) (list x more))
> (foo 1 2 3 4 5)
(1 (2 3 4 5))
> (defn sum (x y) (+ x y))
(fn (x y) (+ x y))
> (sum 1 2)
3
> (fold + '(1 2 3))
6
> (defn even? (x) (== 0 (mod x 2)))
(fn (x) (== 0 (mod x 2)))
> (even? 3)
false
> (even? 4)
true
> (apply + (list 1 2 3))
6
> (map (fn (x) (. System.Math Sqrt x)) (list 1 2 3 4))
(1 1.4142135623731 1.73205080756888 2)
> (filter even? (list 1 2 3 4 5))
(2 4)
> (= "abc" "abc") ; Object.equals()
true
> (def x 1)
  ((fn (x) (println x) (set! x 3) (println x)) 4) ; lexical scoping
  x
4
3
1
> (defn adder (amount) (fn (x) (+ x amount))) ; lexical scoping
  (def add3 (adder 3))
  (add3 4)
7
> (symbol "a")
a
```

#### Iterable
apply, doseq, filter, fold, map work on System.Collections.IEnumerable.
```
> (apply + (filter (fn (x) (or (== 0 (mod x 3)) (== 0 (mod x 5)))) (range 1 1000)))
233168
```

#### Recur
Evaluates the arguments in order. Execution then jumps back to the recursion point, a loop or fn method.

Warning: `recur` does not check the tail position.
```
> (defn sum1 (n s) (if (< n 1) s (recur (- n 1) (+ s n))))
> (defn sum (n) (sum1 n 0))
> (defn sum-nonrecur (n) (if (< n 1) 0 (+ n (sum-nonrecur (- n 1)))))
> (sum 100)
5050
> (sum-nonrecur 100)
5050
> (sum 1000)
500500
> (sum-nonrecur 10000)

Process is terminated due to StackOverflowException.
> (loop (i 0) (when (< i 5) (print i) (recur (+ i 1))))
01234nil
```

### Scope ###
`doseq`, `fn`, `let`, `loop` make new scope.

### List ###
```
> (. (list 2 4 6) get 1)
4
> ((list 2 4 6) 1) ; implicit indexing
4
> (. (list 1 2 3) size)
3
```

### Macro ###
Macro is non-hygienic.

```
> (defmacro infix (a op & more) `(~op ~a ~@more))
nil
> (infix 3 + 4)
7
> (infix 3 + 4 5)
12
> (macroexpand '(infix 3 + 4 5))
(+ 3 4 5)
> (macroexpand '(while true (println 1)))
(loop () (if true (do (println 1) (recur))))
```

### .NET interoperability (from Closhure) ###
```
> (import System) ; System is imported by default.
nil
> (import System.Math) ; Clojure syntax
System.Math
> [(. (Random.) NextDouble) (.NextDouble (new Random))]
(0.207981260590247 0.207981260590247)
> (. Math Floor 1.5) ; class's static method.
1.0
> (. "abc" -Length) ; object's property
3
> (. "abc" get_Length) ; also object's property (C#'s feature)
3
> (. true ToString)
"True"
> [(. System.Math -PI) System.Math/PI]
(3.14159265358979 3.14159265358979)
> (. Closhure.Core -testField)
nil
> (set! (. Closhure.Core -testField) 1) ; set field
  (. Closhure.Core -testField)
1
> (set! Closhure.Core/testField "abc")
  Closhure.Core/testField
"abc"
> (set! (. Closhure.Core -testProperty) 3)
3
> [Closhure.Core/testProperty (. Closhure.Core -testProperty)]
(3 3)
> (. Closhure.Core set_testProperty 4) ; (C#'s feature) method-like: prepend set_ to property name to set
nil
> (. Closhure.Core get_testProperty) ; (C#'s feature) method-like: prepend get_ to property name to get
4
> (str (reify Object (ToString [this] (str "reified object: " this))))
"reified object: System.Object"
> (def h (new System.Collections.Hashtable))
System.Collections.Hashtable
> (. h Add "a" "apple")
nil
> [(. h -Item 0) (. h -Item "a")]
(nil "apple")
```

See the source code for details.
