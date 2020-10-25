using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UXF.UI;

namespace UXF.EditorUtils
{
    [CustomEditor(typeof(AWSCredentials), true)]
    [CanEditMultipleObjects]
    public class AWSCredentialsEditor : Editor
    {

        AWSCredentials cred;

        public void OnEnable()
        {
            cred = (AWSCredentials)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Before using this DataHandler to store data in an online DynamoDB database, you need to set up an Amazon AWS account, DynamoDB, and a Cognito Identity Pool with write access to your database. The article on the Wiki can help:",
                UnityEditor.MessageType.Info);

            if (EditorExtensions.MiddleButton("Go to Wiki page"))
            {
                Application.OpenURL("https://github.com/immersivecognition/unity-experiment-framework/wiki/AWS-DynamoDB-setup");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enter the region of your AWS DynamoDB & Cognito Identity pool. It should look like 'eu-west-2'.", UnityEditor.MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("region"));

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Enter ID of the Cognito Identity Pool you setup to have read/write access to your DynamoDB. It should look like 'eu-west-2:00000000-0000-0000-0000-000000000000'.", UnityEditor.MessageType.Info);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cognitoIdentityPool"));
            
            // Write back changed values and evtl mark as dirty and handle undo/redo
            serializedObject.ApplyModifiedProperties();
        }

    }

}

