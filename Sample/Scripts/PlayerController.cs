using System;
using StatController.Runtime;
using UnityEngine;

namespace StatController.Sample
{
    public class PlayerController : MonoBehaviour
    {
        private Runtime.StatController _statController;
        private Stat _hpStat;
        private Stat _strStat;
        private Stat _spdStat;


        private void Awake()
        {
            _statController = GetComponent<Runtime.StatController>();
        }


        private void Start()
        {
            _hpStat = _statController.GetStat("HP");
            _strStat = _statController.GetStat("Gold");
            _spdStat = _statController.GetStat("Bard Story");
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _hpStat.AddModifier(new StatModifier(3, modifierType: StatModifierType.Additive));
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _strStat.AddModifier(new StatModifier(6, modifierType: StatModifierType.Multiplicative));
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _spdStat.AddModifier(new StatModifier(9, modifierType: StatModifierType.Additive));
            }
        }
    }
}
