# DebugInterface
Simple debug interface for client server live variable evaluation.

[![Build status](https://ci.appveyor.com/api/projects/status/f8hn8mu8v49ovfw9/branch/master?svg=true)](https://ci.appveyor.com/project/memsom/debuginterface/branch/master)

## Motivation
I seem to have re-written this code a number of times over the last 3 or 4 years. It is a very basic idea - when developing, we are constantly needing access to variables, data structures and other bits of runtime info, but in the real world of multi-tier, we often end up killing the process we are trying to debug by placing too many break points in the code to grab various watches. _If only there was a way to get this kind of info at runtime without breaking in to the debugger!?!_ Well - here is my solution. It's dead simple. It's easy to set up, and it really does work. Should you use this in production? Hell no!! It opens up a nasty back door in to your app. But during development and whilst testing, this library can get you a cornucopia of lovely data.

For a mote in depth discussion and sample code, look at this article, see https://www.codeproject.com/Articles/1195657/Simple-DebugInterface for a brief intro I wrote.


