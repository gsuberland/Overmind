# Overmind

Overmind is a cross-platform task launching service. It allows you to set up parameterised tasks (e.g. programs, shell scripts) that can be launched and monitored using a HTTP API.

Overmind is written in C# targeting .NET 6, and is intended to be compatible with any operating system that is supported by the .NET runtime.

## Example

Imagine you're building a media library application and want to create a thumbnail image for newly imported media, for example with this command:

`ffmpeg -i /path/to/media/file.mp4 -ss 00:00:01.000 -vframes 1 /path/to/media/thumb_58384.png`

Rather than synchronously launching ffmpeg from your application, you can set up an Overmind task for this:

```json
{
  "Name": "createthumb",
  "Executable": "/path/to/ffmpeg",
  "Arguments": [
    "-i",
    "@@inputfile@@",
    "-ss",
    "00:00:01.000",
    "-vframes",
    "1",
    "@@inputpath@@/thumb_@@id@@.png"
  ],
  "Parameters": [
    {
      "Name": "inputfile",
      "ValidationBasePath": "/mnt/media/"
    },
    {
      "Name": "inputpath",
      "ValidationBasePath": "/mnt/media/"
    },
    {
      "Name": "id",
      "ValidationRegex": "^[0-9]+$"
    }
  ]
}
```

This defines a task named `createthumb` that launches ffmpeg, taking parameters named `inputfile`, `inputpath`, and `id`.

The `inputfile` and `inputpath` parameters have a validation base path specified, which ensures that the provided path is within the given directory, to prevent directory traversal attacks.

The `id` parameter has a validation regex specified, to ensure that the parameter only contains numbers.

The task can be launched via the HTTP API:

```http
HTTP/1.1 POST /start
Host: 127.0.0.1:11180
Content-Type: application/json
Security-Token: 01234567-89ab-cdef-0123-456789abcdef


{
  "Name": "createthumb",
  "Parameters": {
    "inputfile": "/mnt/media/movies/Alien/Alien.mp4",
    "inputpath": "/mnt/media/movies/Alien",
    "id": "58271"
  }
}
```

The value of the `Security-Token` header is returned by the `/token` API. This is used for cross-site request forgery prevention.

The response will look something like this:

```json
{
  "Id": "69c3dd8b-30a8-4a58-aa72-5b6e7fedf434",
  "TaskName": "createthumb",
  "StartTime": "2022-08-08T04:31:54.4475332Z",
  "EndTime": "2022-08-08T04:31:54.4702573Z",
  "ProcessId": 17236,
  "PlatformExitCode": null,
  "Status": "running",
  "Parameters": {
	"inputfile": "/mnt/media/movies/Alien/Alien.mp4",
	"inputpath": "/mnt/media/movies/Alien",
	"id": "58271"
  },
  "Success": true
}
```

You can track the status of the task by sending a GET request to `/status/[id]`, where `[id]` is the ID field returned in the request. The full status URL is also returned in the `Location` response header when a task is created.

## Getting Started

Build the `Overmind.Server.csproj` project. I use VS Code for development but you can build from the command line easily.

Overmind will look for its overmind.json config file in the current working directory when you run it. You can find an example config file in this repo.

Servers are defined in the config file. You can define multiple HTTP servers running on different ports if you like.

The project runs as a console application. You can run this on Linux/MacOS as a service. On Windows you'll currently need to wrap it with a service wrapper tool, but I plan to add a service too.

Make sure you read the security section of this readme!

## API Conventions

- All requests and responses are in UTF-8 encoded JSON.
- Names are case sensitive.
- All responses from the server include a `Success` field. When `true`, the response will be formatted based on the type of request. When `false`, the response will contain information about the error that occurred.
- HTTP status codes are set to an appropriate value when an error occurs; Overmind does not return an error alongside a 200 status code.

## Security

Since Overmind runs executables and commands based on user input there is an inherent security risk. Exposing an Overmind server instance to anything but the loopback address (i.e. 127.0.0.1) is particularly risky and is not recommended.

Overmind supports HTTP Basic Authentication. You can configure the username and password in the configuration file.

**You are responsible for configuring your tasks to prevent command injection attacks.** If you make a task that launches bash or cmd.exe without validation on your parameters, that is a near guarantee of remote code execution or privilege escalation. Be careful.

Each HTTP request to Overmind needs a `Security-Token` header to be set. You can get this token value by sending a GET request to `/token`. This helps prevent cross-site request forgery (CSRF) attacks. In addition, Overmind does not set an `Access-Control-Allow-Origin` header, so cross-origin XHRs & fetches to Overmind are disallowed.

Overmind tasks run as the same user that Overmind itself runs as. You should run Overmind with the minimum possible privileges. It is strongly recommended that you do not run Overmind as root or SYSTEM.

Overmind's security model assumes that all programs run from tasks properly treat each input argument (i.e. each separate string in the `argv` array) as a whole value, rather than splitting the values up. For example, if `argv[1]` is `"-a -b"`, it should be treated as *one* argument with the value `-a -b`, not two arguments with the values `-a` and `-b`. Programs that violate this assumption are misbehaving, and exposing them as Overmind tasks is risky due to the potential for command injection vulnerabilities.

The `/config` API will return the configuration of the server. The username and password fields will be blanked out, but everything else is visible. Do not assume that a malicious user will not know your task configuration - security through obscurity is not a strong control!

HTTPS support is experimental and untested. You'll need to set it up via the system certificate store. Search for "HttpListener https" for more information.

## Development

The code is heavily commented, so it should be reasonably accessible. I've tried to architect things in a sensible manner without going overboard with overly-enterprisey code.

An overview of the architecture is as follows:

The `Overmind.Messages` class library contains the type definitions for JSON messages sent to / received from the server.

The `Overmind.Server` application is the server.

The core of the HTTP server itself is in `Overmind.Server.Web.WebServer`. Each "service" (usually designated by the first part of the URL after the slash) is handled by a dispatcher class. When a web server is initialised it will call into `DispatchManager`, which looks through the assembly using reflection and registers any dispatch classes it finds. For example, the `/token` request is handled by the `SecurityTokenDispatch` class.

By convention, errors in dispatchers should be signalled by raising an exception that derives from `OvermindException`. That exception class has a field for setting the appropriate HTTP response code, which the web server's dispatch routine picks up on.

Tasks are managed at a high level by `Overmind.Server.Tasks.TaskManager`. The bulk of the task validation and launch logic happens in `TaskInstance`.

The Overmind configuration file (overmind.json) is parsed into types found in the `Overmind.Config` namespace.

## Planned Features

- **.NET client library** - Right now it's just a server with a JSON API over HTTP; I'd like to write a .NET client library to consume it.
- **Completion callbacks** - User-provided URI, likely with some restrictions, to which a HTTP request is sent when the task is completed. This saves polling repeatedly.
- **Task cancellation** - Ability to kill a task from the API.
- **Process information** - Information about the process' runtime (platform dependent) such as memory usage and CPU usage
- (maybe) **Task chaining** - Ability to specify a chain of tasks to perform one after another.

## License & Support

Overmind is released under [MIT license](LICENSE.md).

I have severe ADHD and offer no guarantee of support or maintenance on this software. Issues and PRs are welcome nonetheless. If you've had an issue or PR open for more than a week without a response, you can try poking me [on Twitter](https://twitter.com/gsuberland).

