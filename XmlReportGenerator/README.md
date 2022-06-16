============================================================

XmlReportGenerator
------------------
This is the main library to process the files

appsettings.json:-

InputDirectory - Specify the input directory from where to read the files.

OutputDirectory - This is the Output directory for moving the generated file.

ProcessedDirectory - Program will move the processed file to this directory

ExceptionDirectory -  Program will move the error file (unprocessed due to exception)

OutputFileAppender - Appender for the result file

MaxFilesInSingleRead - For single read progrm will take no of files based on this setting. Default is 10 files.

XmlReportGenerator.Console
---------------------------
Runnable Console program that calls the library functions

XmlReportGenerator.Tests
-------------------------

Test Project

============================================================

Assumptions/Concerns
-------------------------------------

1) File size is not specified. If the file is too big, then we have to use redis cache in OutputGenerator to temporary store and retrieve the data.
   Currently it keeps the data in collection before writing to output file

2) What will happen if the file contain other than specified generator.  The program moves to exception if all generators and invalid.
   If one of the generator is invalid then it will omit the data. Needs clarification what to do in that case

3) Generator Type(Offshore/Onsite) for Wind is decided based on location. So if there is mismatch between WindGenerator name and Location, then output seems bit different

4) Higest Day emissions -> What should happen if there is multiple data for same day for same generator ? Currently the system will overwrite the value.Expected behavior is not specified

5) No information about filename uniqueness. Program assumes only files with distinct name will be moved to the input directory for processing.It overwrites the output if the same file/ file with 
   same name is dropped to the input directory

6) Progrma must have necessary privilege to read/write permission




