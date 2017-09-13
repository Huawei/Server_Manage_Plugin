@echo off
REM --- Windows script
REM --- (if Ant runs out of memory try defining ANT_OPTS=-Xmx512M)

@setlocal
@IF not defined ANT_HOME (
   @echo BUILD FAILED: You must set the env variable ANT_HOME to your Apache Ant folder
   goto end
)
@IF not defined VSPHERE_SDK_HOME (
   @echo BUILD FAILED: You must set the env variable VSPHERE_SDK_HOME to your vSphere Client SDK folder
   goto end
)
@IF not defined FLEX_HOME (
   @echo Using the Adobe Flex SDK files bundled with the vSphere Client SDK
   @set FLEX_HOME=%VSPHERE_SDK_HOME%\resources\flex_sdk_4.6.0.23201_vmw
)
@IF not exist "%VSPHERE_SDK_HOME%\libs\vsphere-client-lib.jar" (
   @echo BUILD FAILED: VSPHERE_SDK_HOME is not set to a valid vSphere Client SDK folder
   @echo %VSPHERE_SDK_HOME%\libs\vsphere-client-lib.jar is missing
   goto end
)

@call "%ANT_HOME%\bin\ant" -f ../src/huawei-vcenter-plugin-ui/build-war.xml

:end
