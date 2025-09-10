# Remote server that supports StatPeak requirements
StatPeak plugin sends statistics gathered during the run to remote server.
By default this remote server is of my dear friend (you are the best!),
[hacktix™](https://github.com/Hacktix).

But what if you choose not to trust us? This is where this small go project
comes in! In short, this is an example server, which you can compile using
golang compiler and run locally or host somewhere else. The goal for this
is to provide a foundation on which YOU can build upon.

## So what is actually needed for me to run the server?
Not much, here's the whole list:
- A way to listen for incoming requests
- 2 endpoints: `/looks` and `/stats`
- Function to validate ticket provided with each POST request (Optional, but highly recommended)

Yep, that's it. This example\_server only provides bare minimum to fulfill
those requirements.
