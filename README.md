# SearchAndDestroy

A very simple, very light-weight program that can find-and-replace a series of patterns in entire directory structures.

## Arguments

```
  -v, --verbose         Set output to verbose messages.

  -r, --replace         Required. Set the find/replace pairs. You must specify this parameter always in pairs, ie the
                        first of the pair is the find pattern, the second of the pair is the replace patter. For
                        example, to add two replaces, you would add the argument --replace "findA" "withA" "findB"
                        "withB", where all strings matching 'findA' are replaced with 'withA' and all strings matching
                        'findB' are replaced with 'withA'.

  -d, --dir             Required. The directory (or directories) to perform the find/replace on.

  -i, --include         The inclusion pattern (or patterns) that all directories and files must match to. To include
                        everything, use '*'.

  -e, --exclude         The exclusion pattern (or patterns) that will skip any included directories and files. This
                        argument has precedance over the inclusion argument.

  -R, --recursively     (Default: true) If set, the directories are traversed recursevily through all sub-directories.

  -D, --dry-run         (Default: false) If set, no actual changes are performed.

  -n, --no-confirm      (Default: false) If set, the confirmation is skipped.

  -s, --slow            (Default: false) If set, a key-press is requested at each traversal through the directories and
                        files. This is useful together with dry-run if you want to walk the actions together with the
                        program before doing a real run.

  -g, --use-git-move    (Default: false) If set, a "git mv" command is attempted if any directories or files are to be
                        renamed.

  --help                Display this help screen.

  --version             Display version information.
```

## Examples

### Rename this program to FindAndReplace:

Command:

```
SearchAndDestroy.exe --dir C:\SearchAndDestroy\src --replace SearchAndDestroy FindAndReplace --include *.* --exclude .* bin obj --verbose --dry-run
```

Explanation:

 - ```--dir C:\SearchAndDestroy\src```: set the directory to do the find-and-replace to ```C:\SearchAndDestroy\src```
 - ```--replace SearchAndDestroy FindAndReplace```: replace all ```SearchAndDestroy``` with ```FindAndReplace```
 - ```include *.*```: include all files matching the pattern ```*.*```
 - ```--exclude .* bin obj```: exclude all files/directories that match the any of the following patterns: ```.*```, ```bin```, ```obj```
 
 Output:
 
 ```
 Verbose:        True
No Confirm:     False
DryRun:         True
Recursively:    True
UseGitMove:     False
Include:        '*.*'
Exclude:        '.*'
Exclude:        'bin'
Exclude:        'obj'
Replace:        'SearchAndDestroy' with 'FindAndReplace'
Directory:      'C:\SearchAndDestroy\src'

Press enter key to continue...

Dir C:\SearchAndDestroy\src...
  Found file C:\SearchAndDestroy\src\Program.cs...
    Replacing 'SearchAndDestroy' with 'FindAndReplace'...
    Found 1 matche(s)
  Found file C:\SearchAndDestroy\src\SearchAndDestroy.csproj...
  Found SearchAndDestroy in file name
  Renaming file from 'SearchAndDestroy.csproj' to 'FindAndReplace.csproj'
    Replacing 'SearchAndDestroy' with 'FindAndReplace'...
    Found 1 matche(s)
  Found file C:\SearchAndDestroy\src\SearchAndDestroy.csproj.user...
  Found SearchAndDestroy in file name
  Renaming file from 'SearchAndDestroy.csproj.user' to 'FindAndReplace.csproj.user'
    Replacing 'SearchAndDestroy' with 'FindAndReplace'...
Excluded dir C:\SearchAndDestroy\src\.vs due to '.*'...
Excluded dir C:\SearchAndDestroy\src\bin due to 'bin'...
Excluded dir C:\SearchAndDestroy\src\obj due to 'obj'...
Dir C:\SearchAndDestroy\src\Properties...
  Found file C:\SearchAndDestroy\src\Properties\launchSettings.json...
    Replacing 'SearchAndDestroy' with 'FindAndReplace'...
    Found 2 matche(s)

Replaced 'SearchAndDestroy' in file contents 4x
Replaced 'SearchAndDestroy' in file name 2x
```