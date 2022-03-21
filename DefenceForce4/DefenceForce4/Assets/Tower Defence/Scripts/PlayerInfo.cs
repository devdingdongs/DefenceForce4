using UnityEngine.UI;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField]
    internal Image profile = null;
    [SerializeField]
    internal Text playername_text = null, awards_title_text = null, coin_text = null, air_kills_text = null, ground_kills_text = null, walls_produced_text = null,
                         wall_destroyed_text = null, turret_destroyed_text = null, turret_build_text = null, detonated_mines_text = null;
}
