# FractalStuff

![release](https://img.shields.io/github/v/release/milan338/FractalStuff?include_prereleases&style=flat-square)
![downloads](https://img.shields.io/github/downloads/milan338/FractalStuff/total?style=flat-square)
![issues](https://img.shields.io/github/issues/milan338/FractalStuff?style=flat-square)
![size](https://img.shields.io/github/repo-size/milan338/FractalStuff?style=flat-square)
![license](https://img.shields.io/github/license/milan338/FractalStuff?style=flat-square)

FractalStuff is a simple app to visualise 3D fractals, built using Unity.

## Navigation

- [Navigation](#navigation)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Limitations](#limitations)
- [Building it Yourself](#building-it-yourself)
  - [Prerequisites](#prerequisites)
  - [Downloading](#downloading)
  - [Building](#building)
- [Contributing](#contributing)
- [Screenshots](#screenshots)
- [ToDo](#todo)
- [License](#license)

## Features

- **Customisable**: Resize, reorder, and recolour fractals on the fly. You can also adjust the number fractal iterations.
- **Expandable**: Easily add definitions for generating new fractals through a simple-to-use extension system.
- **Performant**: View complex fractals with up to 15 iterations with a high framerate.

## Installation

Download a build from the [releases](https://github.com/milan338/FractalStuff/releases) page.

## Usage

 - Create new fractals using the `Select Fractal` dropdown
 - Select a fractal using its button to set the camera orbit to that fractal
 - Right click and drag to orbit around the currently selected fractal
 - Use the scroll wheel to zoom in / out of the currently selected fractal
 - Change fractal colour using the `C` button
 - Remove a fractal using the `-` button
 - Change the base length of a fractal using the `length` slider
 - Change the number of fractal iterations using the `iterations` slider
 - Change x, y, z coordinates of the fractal using the `x`, `y`, and `z` sliders accordingly
 - Adjust the background colour using the `Background Color` button
 - Export the current scene with the `Export PNG` button
 - Return to a 'home view' using the `Home View` button
 - Exit the app using the `escape` key

## Limitations

Due to how meshes are combined, for any fractal, there will always be a limit of 2^32 vertices.
This means a maximum of 15 iterations for the Sierpiński tetrahedron fractal, and this number will reduce as the number of vertices per each fractal generator increases.

Unfortunately for some devices, the max number of vertices per mesh supported is only 2^16, since not all GPUs support a 32 bit [mesh index format](https://docs.unity3d.com/ScriptReference/Rendering.IndexFormat.html).
FractalStuff forces this limit to 2^32 to support higher numbers of iterations, but this means that some devices just won't be supported.

## Building it Yourself

### Prerequisites

#### Software

- **Unity** 2020 builds or higher

#### Libraries

- **TextMesh Pro**
- [**HSV Color Picker**](https://github.com/judah4/HSV-Color-Picker-Unity)
- [**UnityStandaloneFileBrowser**](https://github.com/gkngkc/UnityStandaloneFileBrowser) (Included in repo)

### Downloading

```
git clone https://github.com/milan338/FractalStuff.git
```
### Building

Make sure the project API compatibility level is set to `.NET 4.x`.

***Edit → Project Settings → Player → API Compatibility Level → .NET 4.x***

To build, go to ***File → Build Settings*** and select your target platform, after which you can simply click the ***build*** button.

## Contributing

Before contributing, please ensure you've read the [code of conduct](CODE_OF_CONDUCT.md).

If you'd like to contribute to the project, please see the [guidelines](CONTRIBUTING.md).

## Screenshots

![main](/Images/screenshot_main.png)

## ToDo

- [ ] Faster fractal generation
- [ ] Functional perspective cube
- [ ] More fractals
- [ ] CI pipelines

## License

[GNU General Public License (version 3)](LICENSE)
