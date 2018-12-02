# PGL2_Fiser_Exam
This is unmodified PGL2_Fiser_FinalExam release commit from my local repo. 
Same project image can be found on my public onedrive page.

## Requirements
* .NET 4.7
* Windows 10 x64/x32 (Linux NOT tested => I did use WPF and no WIN32.api direct calls, so it should be fine?)
* DirectDraw supported GPU (Nearly all GPUs support this.) (WPF requirement)

## Introduction
This project will be example of stag.ws services usage.

I wont bother you with additional details, so here is 
a list of things you can find here:
* Advanced OOP
* MVC architecture
* Generics
* Parallel programming => Threads, delegates, async waiting
* =||=, => Task usage, custom exceptions idea (handling) => rasing ex. wihout failing application.
* Json Parsing
* WS stag services usage
* Login example
* "Cache" implementation (not finished)
* GUI? => Lite version introduced :). (Just quick xaml edit.. not proper way)

## Current Usage
* You are currently able to get students by their osId,
  get their schedule if logged in.
* Get teacher by name, filter getted list by exact name & surname.
* Get common schedule actions of specified students. (and filter
them by teacher if one wishes to do so..)
* (For other examples, check my manager and services classes. They 
have most usable functionality impl. in them.)
