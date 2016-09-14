# Roadmap

- [ ] DeploymentItem attribute handle
- [ ] Assert.Fail handle
- [ ] Assert text messages removal

Features that have a checkmark are complete and available for
download in the
[CI build](http://vsixgallery.com/extension/dd067b6f-96bd-4d0d-be4b-cd0d62fc66b8/).

# Changelog

These are the changes to each version that has been released
on the official Visual Studio extension gallery.

## 1.0

**2016-09-14**

- [x] Initial release
- [x] Removes TestClass attribute
- [x] Replace MSTest Asserts calls with XUnit.Asserts calls
- [x] Replace Ignore attributes with Skipped Fact attributes
- [x] Replace TestCategory attributes with Trait attributes
- [x] Replace ExpectedException with Assert.Throws<> wrapped block
- [x] Remove MSTest reference and add XUnit nuget packages
