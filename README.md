# LICC

[![Actions Status](https://github.com/pipe01/LICC/workflows/CI/badge.svg)](https://github.com/pipe01/LICC/actions)

Library for Implementing C# Commands

---

LICC is a tool for parsing console commands in C#. You simply add an attribute to a method, and that method becomes a command.

```csharp
[Command]
static void SayRepeating(string text, int repetitions = 1)
{
    if (repetitions < 1)
        throw new ArgumentOutOfRangeException(nameof(repetitions), 1);
    
    for (int i = 0; i < repetitions; i++)
        LConsole.WriteLine(text);
}
```

LICC can now parse command text input as a call to that method, like so:

```
> sayrepeating "hello world!"
hello world

> sayrepeating "and another one" 5
and another one
and another one
and another one
and another one
and another one

> sayrepeating boobs -1
ArgumentOutOfRangeException: the argument 'repetitions' must be at least 1
```

LICC provides a static class which parses command text input. You have to implement a LICC frontend yourself, providing a method of input and output for the user.

If you want to use LICC in your command-line project, check out [LICC the Console](./LICC.Console), an open-source LICC frontend for the system console. <!--If you want to implement a LICC frontend yourself, check out [The Wiki](todo make repo public so it can have a wiki) for how to do that.-->

---

LICC was created for the game [Logic World](https://logicworld.net/), where the console frontend in this repo is used in the standalone server application. We have also developed a LICC frontend for the Unity game engine called [Fancy Pants Console](https://www.youtube.com/watch?v=QDp5wE1Se6o) which will eventually be open sourced.
