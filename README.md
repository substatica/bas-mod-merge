# Blade & Sorcery: Nomad Mod Merge

This utility will merge two bas.jsondb mod files for usage with Blade & Sorcery: Nomad.

**Usage:**

  `bas-mod-merge.exe [unmodified bas.jsondb] [dominant mod] [subordinate mod]`
  
**How It Works**

This utility compares two modified bas.jsondb files to a reference bas.jsondb file and creates a merged bas.jsondb file.

A file is determined to be modified if it differs from the same file contained in the reference bas.jsondb file.

If both mods contain modifications to the same file, the dominant mod file will be used.

The resulting merged bas.jsondb will be written in an output directory named **merge_output**.

Dominant mod and subordinate mod files can be compressed or extracted bas.jsondb files.

**Caution**

Results of merging mods may lead to unpredicatable gameplay and/or instability.
