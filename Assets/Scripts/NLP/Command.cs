using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public class RecognizedEntity
    {
        public string Role { get; private set; }
        public string Text { get; private set; }

        public RecognizedEntity(string role, string text)
        {
            Role = role;
            Text = text;
        }
    }

    public string Intent { get; private set; }

    public List<RecognizedEntity> RecognizedEntities { get; private set; }

    public Command(string intent, List<RecognizedEntity> entities)
    {
        Intent = intent;
        RecognizedEntities = entities;
    }

    public List<string> GetEntitiesWithRole(string desiredRole)
    {
        List<string> recognizedEntitiesWithRole = new List<string>();

        foreach (RecognizedEntity recognizedEntity in RecognizedEntities)
        {
            if (recognizedEntity.Role.ToLower() == desiredRole.ToLower())
            {
                recognizedEntitiesWithRole.Add(recognizedEntity.Text);
            }
        }

        return recognizedEntitiesWithRole;
    }
}
