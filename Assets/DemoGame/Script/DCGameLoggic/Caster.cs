﻿using System.Collections.Generic;
using DC.SkillSystem;
using DC.ActorSystem;
using UnityEngine;
using ValueType = DC.ValueSys.ValueType;

namespace DC.GameLogic
{
    public class Caster : GameElement, ICaster
    {
        public CastMsg ConsumeEnough(ISkill skill)
        {
            var values = GetActor().GetValueComponent();
            var mp = values.GetValue(ValueType.mp);
            var consumes = skill.GetSkillCfg().GetConsumes();

            throw new System.NotImplementedException();
        }

        public CastMsg CdEnough(ISkill skill)
        {
            throw new System.NotImplementedException();
        }

        public Transform GetCastTransform(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool Cast(ISkillCfg skillCfg)
        {
            var castCfg = GetCastSystem().GetDefaultCastCfg(skillCfg);
            return Cast(skillCfg, castCfg);
        }

        public bool Cast(ISkillCfg skillCfg, ICastCfg castCfg)
        {
            var skill = GetSkillSystem().CreateSkill(skillCfg);
            skill.SetCaster(this);
            skill.SetCastCfg(castCfg);

            /*
             判断是否可以释放
             buff
                 沉默
                 晕眩
             能量
             cd
             */

            var buffAllowCast = BuffAllowCast(skill);
            if (buffAllowCast.Error)
            {
                return false;
            }

            var consumeEnough = ConsumeEnough(skill);
            if (consumeEnough.Error)
            {
                return false;
            }

            if (CdEnough(skill).Error)
            {
                return false;
            }

            skill.Apply();
            return true;
        }

        public void SetSkillActive(ISkillCfg skillCfg, bool active)
        {
            throw new System.NotImplementedException();
        }

        public List<ICastCfg> GetActiveCastCfgs()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateCastConfig(ICastInput input)
        {
            throw new System.NotImplementedException();
        }

        public List<ISkill> GetActiveSkills()
        {
            throw new System.NotImplementedException();
        }

        public ISkill GetActiveSkill()
        {
            throw new System.NotImplementedException();
        }

        public IActor GetActor()
        {
            throw new System.NotImplementedException();
        }

        public CastMsg BuffAllowCast(ISkill skill)
        {
            var ownerBuffs = GetActor().GetOwnerBuffs();
            var rejectBuff = ownerBuffs.Find((buff => !buff.AllowCast(skill)));
            if (null == rejectBuff)
            {
                return CastMsg.s_Suc;
            }

            return new CastMsg(CastMsgType.buff_reject, rejectBuff);
        }
    }
}