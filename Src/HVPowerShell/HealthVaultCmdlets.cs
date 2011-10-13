//
// Copyright 2011 Vitraag LLC.
//
// Author : Vaibhav Bhandari (vaibhavb@vitraag.org)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Management.Automation;

using Microsoft.Health;
using Microsoft.Health.ItemTypes;

namespace HealthVault.PowerShell.Cmdlets
{
    
    [Cmdlet("Grant", "HVaccess")]
    public class GrantHVaccess : PSCmdlet
    {
        private Guid HVPowerShellGuid = new Guid("9aa44e59-faa9-417e-b419-0e627c0d1b91");
        private Uri HVPlatform = new Uri("https://platform.healthvault-ppe.com/platform/");
        private Uri HVShell = new Uri("https://account.healthvault-ppe.com/");

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HealthClientApplication.Create(
                Guid.NewGuid(), HVPowerShellGuid, HVShell, HVPlatform);

            // Verify the application instance.
            //   Create a new instance if necessary.

            if (clientApp.GetApplicationInfo() == null)
            {
                // Create a new client instance.                  
                clientApp.StartApplicationCreationProcess();

                // A new client instance always requests authorization from the 
                //   current user using the default browser.

                // Wait for the user to return from the shell.
                if (ShouldContinue("Is Auth done - (Y)?", "Is auth done?", 
                             ref _yesToAll, ref _noToAll))
                {
                    // Store the SODA client details                    
                    StringBuilder data = new StringBuilder();
                    data.Append(clientApp.ApplicationId.ToString());
                    using (StreamWriter sw = new StreamWriter(HvShellUtilities.GetClientAppAuthFileNameFullPath()))
                    {
                        sw.Write((data));
                    }                    
                    List<PersonInfo> authorizedPeople = 
                        new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());
                    WriteObject(authorizedPeople);
                }
            }
        }
        private bool _yesToAll, _noToAll;
    }


    [Cmdlet("Get", "Personinfo")]
    public class GetPersonInfo : PSCmdlet
    {

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            if (clientApp.GetApplicationInfo() != null)
            {
                List<PersonInfo> authorizedPeople =
                        new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());
                WriteObject(authorizedPeople);
            }
        }
    }

    [Cmdlet("Get", "Things")]
    public class GetThing: PSCmdlet
    {
        [Parameter(Position = 0)]
        [ValidateNotNullOrEmpty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _type;

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();

            List<PersonInfo> authorizedPeople = new List<PersonInfo>(clientApp.ApplicationConnection.GetAuthorizedPeople());

            // Create an authorized connection for each person on the 
            //   list.             
            HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(
                authorizedPeople[0].PersonId);
               
            // Use the authorized connection to read the user's default 
            //   health record.
            HealthRecordAccessor access = new HealthRecordAccessor(
                authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

            // Search the health record for basic demographic 
            //   information.
            //   Most user records contain an item of this type.
            HealthRecordSearcher search = access.CreateSearcher();

            search.Filters.Add(new HealthRecordFilter(HvShellUtilities.NameToTypeId(Type)));

            foreach (Object o in search.GetMatchingItems())
            {
                WriteObject(o);
            }

        }



    }

    /// <summary>
    /// Currently only supports Weight.
    /// </summary>
    [Cmdlet("Add", "Things")]
    public class PutThings : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _type;

        [Parameter(Position = 1, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public int Value
        {
            get { return _value; }
            set { _value = value; }
        }

        private int _value;

        protected override void ProcessRecord()
        {
            HealthClientApplication clientApp = HvShellUtilities.GetClient();
            List<PersonInfo> authorizedPeople = new List<PersonInfo>
                (clientApp.ApplicationConnection.GetAuthorizedPeople());
            // Create an authorized connection for each person on the 
            //   list.             
            HealthClientAuthorizedConnection authConnection = clientApp.CreateAuthorizedConnection(
                authorizedPeople[0].PersonId);

            // Use the authorized connection to read the user's default 
            //   health record.
            HealthRecordAccessor access = new HealthRecordAccessor(
                authConnection, authConnection.GetPersonInfo().GetSelfRecord().Id);

            Weight weight = new Weight();
            weight.Value = new WeightValue(Value / 2.2, 
                new DisplayValue(Value, "pounds"));

            access.NewItem(weight);
        }
    }
}

public class HvShellUtilities
{

    private static Guid _HVPowerShellGuid = new Guid("9aa44e59-faa9-417e-b419-0e627c0d1b91");
    private static Uri _HVPlatform = new Uri("https://platform.healthvault-ppe.com/platform/");
    private static Uri _HVShell = new Uri("https://account.healthvault-ppe.com/");

    public static string GetClientAppAuthFileNameFullPath()
    {
        // Write information to local store for this application
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //string appName = "HVSH";
        string fileName = "application" + ".hv" + ".txt";
        return docPath + @"." + fileName;
    }

    public static Guid GetClientId()
    {
        string s;
        using (StreamReader r = new StreamReader(new FileStream(GetClientAppAuthFileNameFullPath(), FileMode.Open)))
        {
            s = r.ReadToEnd();
        }
        return new Guid(s);
    }

    public static HealthClientApplication GetClient()
    {
        HealthClientApplication clientApp = HealthClientApplication.Create(
            HvShellUtilities.GetClientId(), _HVPowerShellGuid, _HVShell, _HVPlatform);
        return clientApp;
    }

    public static Guid NameToTypeId(string name)
    {
        switch (name)
        {
            case "weight":
                return Weight.TypeId;
            case "bp":
                return BloodPressure.TypeId;
            case "bg":
                return BloodGlucose.TypeId;
            case "exercise":
                return Exercise.TypeId;
            case "basic":
                return Basic.TypeId;
            default:
                return BasicV2.TypeId;
        }
    }
}

