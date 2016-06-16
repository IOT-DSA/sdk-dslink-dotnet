#!/usr/bin/env bash
nuget install NUnit.Console -Version 3.2.1 -OutputDirectory packages
mono packages/NUnit.Console.3.2.1/tools/nunit3-console.exe UnitTests/bin/Release/UnitTests.dll
exit $?
