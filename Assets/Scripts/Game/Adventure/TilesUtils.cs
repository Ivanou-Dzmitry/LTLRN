using SuperTiled2Unity;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilesUtils : MonoBehaviour
{
     public string[] GetCustomPropertiesFromObject(Collision2D collision)
    {
        try
        {
            SuperCustomProperties customProperties = collision.collider.GetComponentInParent<SuperCustomProperties>();

            Debug.Log(collision.collider.name);

            if (customProperties != null && customProperties.m_Properties != null)
            {
                // Get specific properties by name
                string nameValue = null;
                string typeValue = null;

                foreach (var prop in customProperties.m_Properties)
                {
                    if (prop.m_Name == "Name")
                    {
                        nameValue = prop.GetValueAsString();
                    }
                    else if (prop.m_Name == "Type")
                    {
                        typeValue = prop.GetValueAsString();
                    }
                }

                //Debug.Log($"{nameValue}/ {typeValue}");

                if (nameValue == null || typeValue == null)
                    return null;

                return new string[] { nameValue, typeValue };
            }
            return null;
        }
        catch
        {
            Debug.LogWarning("No SuperCustomProperties found on the collided tilemap.");
            return null;
        }
    }

    public string[] GetCustomTileProperties(Tilemap currentTilemap, Vector3Int tilePos)
    {
        if (currentTilemap == null)
            return null;

        //get current tile
        TileBase tile = currentTilemap.GetTile(tilePos);

        if (tile == null)
            return null;

        //get custom properties from tile
        SuperTile superTile = tile as SuperTile;

        if (superTile == null || superTile.m_CustomProperties == null)
            return null;

        //for custom params
        string[] customData = new string[2];

        foreach (var prop in superTile.m_CustomProperties)
        {
            //get name
            if (prop.m_Name == "Name")
                customData[0] = prop.GetValueAsString();
            //gett type
            if (prop.m_Name == "Type")
                customData[1] = prop.GetValueAsString();
        }

        if (customData[0] == null || customData[1] == null)
            return null;

        return customData;
    }


    public TextAsset GetDialogue(Collider2D collider)
    {
        //Debug.Log(collider.gameObject.name);

        ADV_DialogueTrigger trigger = collider.GetComponentInParent<ADV_DialogueTrigger>();

        if (trigger != null)
        {            
            Debug.Log(trigger.inkJSON.text);

            return trigger.inkJSON;
        }
            

        return null;
    }
}
