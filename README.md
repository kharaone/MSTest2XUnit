# MSTest2XUnit

<!-- Replace this badge with your own-->
[![Build status](https://ci.appveyor.com/api/projects/status/8ud7ryay22re56ij/branch/master?svg=true)](https://ci.appveyor.com/project/kharaone/mstest2xunit/branch/master)

<!-- Update the VS Gallery link after you upload the VSIX-->
Download this extension from the [VS Gallery](https://visualstudiogallery.msdn.microsoft.com/[GuidFromGallery])
or get the [CI build](http://vsixgallery.com/extension/dd067b6f-96bd-4d0d-be4b-cd0d62fc66b8/).

---------------------------------------

This extension allows to migrate a test suite written with MSTest to XUnit.net 2.
It is based on [dotnet/codeformatter](https://github.com/dotnet/codeformatter) with some additional converters. The dotnet converter didn't remove some elements and were letting too much manual refactoring that could have still been automated.

See the [changelog](CHANGELOG.md) for changes and roadmap.

## Features

- Removes TestClass attribute
- Replace MSTest Asserts calls with XUnit.Asserts calls
- Replace Ignore attributes with Skipped Fact attributes
- Replace TestCategory attributes with Trait attributes
- Replace ExpectedException with Assert.Throws<> wrapped block
- Remove MSTest reference and add XUnit nuget packages

### TestClass attribute
Before launching the command:   
![Testclass Before](MSTest2XUnit/Resources/testclass_before.png)    
![Testinitialize Before](MSTest2XUnit/Resources/testinitialize_before.png)    
![Testcleanup Before](MSTest2XUnit/Resources/testcleanup_before.png)    
After the command:   
![Test Setup After](MSTest2XUnit/Resources/test_setup_after.png)

### Ignore attributes
Before launching the command:    
![Ignore Before](MSTest2XUnit/Resources/ignore_before.png)    
After the command:   
![Ignore After](MSTest2XUnit/Resources/ignore_after.png)    

### Assert calls
Before launching the command:    
![Assert Before](MSTest2XUnit/Resources/assert_before.png)    
After the command:   
![Assert After](MSTest2XUnit/Resources/assert_after.png)

### TestCategory attributes
Before launching the command:    
![Testcategory Before](MSTest2XUnit/Resources/testcategory_before.png)    
After the command:    
![Testcategory After](MSTest2XUnit/Resources/testcategory_after.png)    

### ExpectedException attribute
Before launching the command:    
![Expectedexception Before](MSTest2XUnit/Resources/expectedexception_before.png)    
After the command:    
![Expectedexception After](MSTest2XUnit/Resources/expectedexception_after.png)

### References
Before launching the command:    
![Using Before](MSTest2XUnit/Resources/using_before.png)    
![Reference Before](MSTest2XUnit/Resources/reference_before.png)    
After the command:    
![Using After](MSTest2XUnit/Resources/using_after.png)    
![Reference After](MSTest2XUnit/Resources/reference_after.png)    

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.