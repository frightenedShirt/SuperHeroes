
public class PlayerModel
{
    public float m_IcePower { get; private set; }
    public float m_Strength { get; private set; }
    public float m_FirePower { get; private set; }
    public float m_Speed { get; private set; }

    public SuperPowers m_Ability { get; private set; }

    //public PlayerModel(AbilitiesSO _data)
    //{
    //    m_IcePower = _data.IcePower;
    //    m_Strength = _data.Strength;
    //    m_FirePower = _data.FirePower;
    //    m_Speed = _data.Speed;
    //    CheckForPowers();
    //}

    private void CheckForPowers()
    {
        if(m_FirePower > 4)
        {
            m_Ability = SuperPowers.Fire;
        }
        else if(m_Speed > 0)
        {
            m_Ability = SuperPowers.Dash;
        }
        else if(m_Strength > 0)
        {
            m_Ability = SuperPowers.Hulk;
        }
        else if(m_IcePower > 0)
        {
            m_Ability = SuperPowers.Freeze;
        }
        else
        {
            return;
        }
    }
}
