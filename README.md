# nerve

[![Build and Test](https://github.com/nerve-dns/nerve/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/nerve-dns/nerve/actions/workflows/dotnet.yml)

nerve is a modern ad blocking DNS forwarder with high performance and simplicity in mind from the start.

It's a minimal implementation for now of the DNS specification ([RFC-1035](https://datatracker.ietf.org/doc/html/rfc1035)).

Currently under active development.

> [!CAUTION]
> Use only for private networks and not as a public internet facing DNS forwarder.  
> Some specifications of the mentioned DNS RFC and from other related ones are missing.

## Content

* [Features](#features)
* [Installation](#installation)
    * [Quickstart](#quickstart)
    * [Manual](#manual)
    * [From source](#manual)
* [Config](#config)
* [Supported DNS resource records](#supported-dns-resource-records)
* [Contributing](#contributing)
* [License](#license)

## Features

- [X] basic UI for stats and management
- [X] encrypted communication with upstream resolvers
  - [X] DNS over HTTPS (DoH)
  - [X] DNS over TLS (DoT)
- [X] blocklists and allowlists
  - [X] URL and file path support
  - [X] domain and hosts format supported
  - [X] daily auto refresh
- [X] multiple upstream resolver
  - [X] round-robin between them to spread DNS traffic to multiple resolvers
- [X] basic query log privacy mode
  - [X] log everything
  - [X] hide domains
  - [X] hide client
  - [X] anonymous (hide all)

## Installation

### Quickstart

TODO

### Manual

### From source

## Config

TODO

## Supported DNS resource records

## Contributing

Any contibutions are greatly appreciated.
Just fork the project, create a new feature branch, commit and push your changes and open a pull request.

## License

Distributed under the BSD 3-Clause License. See LICENSE for more information.
