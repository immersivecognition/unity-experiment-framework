using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UXF
{

    [CreateAssetMenu(fileName = "MyAWSCredentials", menuName = "UXF/AWS Credentials", order = 1)]
    public class AWSCredentials : ScriptableObject
    {
        public string region;
        [TextArea]
        public string cognitoIdentityPool;

        public bool CheckIfValidFormat()
        {
            if (region == string.Empty)
            {
                Utilities.UXFDebugLogError("Region in AWS Credentials is blank! Please supply your region (e.g. eu-west-2)");
                return false;
            }
            if (cognitoIdentityPool == string.Empty)
            {
                Utilities.UXFDebugLogError("Cognito Identity Pool in AWS Credentials is blank! Please supply your Cognito Identity Pool (e.g. eu-west-2:00000000-0000-0000-0000-000000000000)");
                return false;
            }
            return true;
        }
    }

}