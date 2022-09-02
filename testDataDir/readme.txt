This directory contains files needed to make the tar files used by p4api.net-unit-test
These instructions are a reminder for how I converted u.exe into u.tar to provide a cross platform set of 
files for the unit test.  Once I get this conversion right, it shouldn't be needed again

It is important that the tar files be made with the --format=posix option.
This way the windows tar (bsdtar)  will extract them to utf16 filenames appropriately.
And your russian filenames won't go missing.

Given the original a.exe in this directory, here's what to do (cygwin tools are assumed to be installed).

# extract the original file trees from executable
mkdir a
cd a
..\a.exe   (will extract all files)

# make stuff writable
chmod +w journal
chmod +w p4d.log
chmod +w checkpoint.*

# remove files which cause problems for tests
rm journal p4d.log

# change all checkpoint files which use "cmd" in a trigger to use "touch" (a cross platform replacement) instead
vi checkpoint.*

# check that you got them all
grep cmd checkpoint.*

# now tar it up (with --format=posix)

tar --format=posix -c -v -f ..\a.tar *.*

# do the same steps above for u and s3
