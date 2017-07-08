# Starbound Animated Signs
A simple tool that generates `customsign` items from image files.

## Table of Contents
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Planned](#planned)
- [Contributing](#contributing)

## Features
* Convert a set of images of equal dimensions to animated custom signs.
* Convert animated GIF files to animated custom signs.
* `/spawnitem` commands for all generated signs.
* Set animation speed in frames per second.
* Set sign light color (leave empty for no lighting).

## Installation
* [Download](https://github.com/Silverfeelin/Starbound-AnimatedSigns/releases) and unpack the contents of the latest release.

## Usage
* Open the application (`AnimatedSigns.exe`, located where you unpacked the release).
* Select `Browse...` and select your image file(s)
  * You can also drag image files directly into the empty text field.
* Confirm the order of your frames in the text field. Each line should contain the file path to one image.
* Configure the FPS (frames per second) of the animation. `60` is the maximum value.
* Configure the sign light.
  * Should be a 6 digit hexadecimal string. Leave empty for no lighting.
* Press `Create and Copy` or `Create and Export`.

### Copy
* Check your clipboard for the results after the tool is done.
  * The clipboard contains the `/spawnitem` commands for each sign. You'll want to paste this in a text editor first for further use.
* Paste each `/spawnitem` command in singleplayer. You may need to enable `/admin` first
  * The item will appear at your cursor, so hover your cursor near your character!

### Export
* You will be asked to save a file somewhere. The folder and file name will be used as a base. Each sign will be generated as a new file named in `<path>\<name>[x,y].json`.
* Import the signs using StarCheat, or whatever other tools you have.

## Planned
I don't really have anything planned right now, maybe a live preview or something.

[Feature requests I have accepted](https://github.com/Silverfeelin/Starbound-AnimatedSigns/issues?q=is%3Aopen%20assignee%3ASilverfeelin%20label%3Aenhancement) can be considered 'planned'.

## Contributing
> Before creating a new enhancement request or bug report, please confirm that they do not already exist.

If you have any suggestions or feedback that might help improve this tool, please do create a new issue tagged enhancement!

* [Overview of enhancement requests.](https://github.com/Silverfeelin/Starbound-AnimatedSigns/issues?q=label%3Aenhancement%20)
* [Create enhancement request.](https://github.com/Silverfeelin/Starbound-AnimatedSigns/issues/new?labels=enhancement)

If you have found any bugs in the tool, please report them in a new issue tagged bug!.

* [Overview of bug reports.](https://github.com/Silverfeelin/Starbound-AnimatedSigns/issues?q=label%3Abug%20)
* [Create bug report.](https://github.com/Silverfeelin/Starbound-AnimatedSigns/issues/new?labels=bug)

You can also create [pull requests](https://github.com/Silverfeelin/Starbound-AnimatedSigns/pulls) and contribute directly to the tool!
