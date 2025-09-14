# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Tracking of distance frisbee has flown
- Tracking duration and times bugles have been used
- Tracking of various item uses

## [0.6.0]

### Added

- Tracking of current outfit of the character
- Tracking of length of placed ropes during the run
- Tracking length of chains paths created/placed
- Tracking of campfires lit by local player
- Tracking for whether local player escaped the island or not
- Tracking of amount of ticks removed (from self or from others)

### Changed

- Internal tracking of consumed items, to avoid issues with different languages

## [0.5.0]

### Added

- Tracking of eaten items (per item)
- Tracking of movement based statistics (distance walked, climbed, fallen, etc.)

## [0.4.0]

### Added

- Harmony patches for reacting to items being thrown/released, grabbed and cooked
- Harmony patch to send statistics when player "quits"

### Changed

- Stats are actually being sent to default external server now
- Stats wont be sent, when local player quits while in Airport scene

## [0.3.0]

### Added

- Debug GUI for currently accumulated statistics
- Steam Utility for user validation
- Remote server utility for data storage
- Accumulated statistics are now sent to remote server when run ends or local player disconnects

### Changed

- Accumulated statistics will reset when run is started

## [0.2.0]

### Added

- Harmony patches to listen for affliction status changes
- Harmony patches to count amount of faints, deaths, revives and jumps
- Harmony patch for keeping track of the amount of luggages opened by local player

## [0.1.0]

### Added

- Config entries to configure where stat packages will be sent to
