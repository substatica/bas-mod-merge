# Blade & Sorcery: Nomad Mod Merge

This utility will merge two bas.jsondb mod files for usage with Blade & Sorcery: Nomad.

**Usage:**

  `bas-mod-merge.exe [unmodified bas.jsondb] [dominant mod] [subordinate mod]`
  
**Examples:**

###### PowerShell

`PS C:\>.\bas-mod-merge.exe ".\bas.jsondb" ".\modone.zip" ".\modtwo.zip"`
`PS C:\>.\bas-mod-merge.exe ".\bas.jsondb" ".\modone\bas.jsondb" ".\modtwo\bas.jsondb"`

###### Command Prompt

`C:\>bas-mod-merge.exe ".\bas.jsondb" ".\modone.zip" ".\modtwo.zip"`
`C:\>bas-mod-merge.exe ".\bas.jsondb" ".\modone\bas.jsondb" ".\modtwo\bas.jsondb"`

**How It Works**

This utility compares two modified bas.jsondb files to a reference bas.jsondb file and creates a merged bas.jsondb file.

A file is determined to be modified if it differs from the same file contained in the reference bas.jsondb file.

If both mods contain modifications to the same file, the dominant mod file will be used.

The resulting merged bas.jsondb along with a log file will be written in an output directory named **output**.

Dominant mod and subordinate mod files can be compressed or extracted bas.jsondb files.

**Limitations**

This utility does not account for deleted/added files or any mods other than those affecting solely bas.jsondb.

**Caution**

Results of merging mods may lead to unpredicatable gameplay and/or instability.

**FAQ**

**Q: The utility is stopping without error trying to extract an archive, what can I do?**  
A: The utility is unable to extract the type of archive being provided to it. Try manually extracting both mod files and pointing the utility directly at the modded bas.jsondb files.

**Q: I'm getting an error, what can I do?**  
A: Ensure that the three commandline parameters, the three required filenames, all are encapsulated in double quotes and are separated by a space as seen in the examples above.

**Q: Why is some mod functionality not working in game?**  
A: Only one mod can change each file, if mods include the same file only the dominant mod file will be used. Some functionality of the subordinate mod may not be included. In addition, mods may have unexpected gameplay when combined.

**Q: Can I combine more than two mods?**  
A: Yes. Take the output file from combining two mods and use it as the reference to run bas-mod-merge again. Using the same mod file for both dominant and subordinant will have the effect of combining a single mod with the reference file.
