# MARC
## C# library to parse MARC bibliographic records.

A simple library written in C# to read and display MARC records.

MARC records are the file format used to store information about library books; for example, a work's title, author, publisher, etc.
The program supports records with field data encoded in UTF-8 as well as [MARC-8](https://en.wikipedia.org/wiki/MARC-8).

## Requirements
- Windows operating system
- Visual Studio IDE. Download the free version [here](https://visualstudio.microsoft.com/vs/community/).

## Install

1. Install Visual Studio.
2. Clone the repository.
```
git clone https://github.com/pqin/MARC.git
```
3. Open the new 'MARC' folder and double-click on MARC.sln.
4. The project should open in Visual Studio.
5. Build the project.

## Usage
1. From the 'File' menu, click 'Open'.
2. Browse to the desired MARC record file (*.mrc). Open the file.
3. Wait for the file to load. Click 'Cancel' on the loading dialog to cancel the operation.
4. To view a specific record, click its entry from the list on the left. The record and its fields will display on the right.

## Issues

## Future Features
- support for editing of records
- support for writing records back to disk

## License
Licensed under the GPL-3.0 license.

## References
- [Library of Congress - MARC Standards](https://www.loc.gov/marc/)
