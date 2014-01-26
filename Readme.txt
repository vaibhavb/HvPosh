HealthVaul Powershell (HvPosh)
------------------------------

HvPosh is a powershell module which enables a user to interact with HealthVault using powershell.

Update:1/25/2014 - Please report bugs, a next rev is in works.

Installing the module
----------------------
Please note: This powershell module work with Powershell 2.0. Powershell 2.0 is installed by default on Windows 7, Windows Server 2008 and up.

1. Copy the HvPosh directory to your My Document\WindowsPowershell\Modules directory
2. Start your powershell and import the module
Powershell > import-module HvPosh

Voila! you are ready to go!


Getting Started
---------------

One you have the powershell module load you need to give access to HealthVault. This is a one time only thing and the module remember's your selection -
Powershell > Grant-Hvacces

Follow the sign up process in your browswer and upon successful completion you can use powershell natively!

Available Functionality
-----------------------

Currently this module support the following HealthVault commands.
* Get-Things
* Add-Things
* Get-Personinfo


Working with the source
-----------------------
The source for this project is available in the Src\ directory and is distributed under the Apache 2.0 license.

Copyright (c) Vaibhav Bhandari, Vitraag LLC, 2011.
