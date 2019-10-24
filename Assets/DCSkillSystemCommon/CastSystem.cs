﻿using System;
using System.Collections.Generic;
using UnityEngine;
using DC.ActorSystem;
using ValueType = DC.ValueSys.ValueType;
using DC.DCPhysics;

namespace DC.SkillSystem
{

    public interface ICastSystem
    {
        CastCfg GetDefaultCastCfg(SkillCfg skillCfg);
    }

    /// <summary>
    /// 转换ui或者游戏中的input为技能理解的输入
    /// </summary>
    public interface ICastInput
    {
        List<IActor> GetTargets();
        void SetTargets(List<IActor> targets);
    }

    public interface ICaster
    {
        /// <summary>
        /// cast with default cast config
        /// </summary>
        /// <param name="skillCfg"></param>
        /// <returns></returns>
        bool Cast(SkillCfg skillCfg);

        /// <summary>
        /// cast with specific cast config
        /// </summary>
        /// <param name="skillCfg"></param>
        /// <param name="castCfg"></param>
        /// <returns></returns>
        bool Cast(SkillCfg skillCfg, CastCfg castCfg);

        /// <summary>
        /// active skill or not
        /// active then player may adjust cast config
        /// </summary>
        /// <param name="skillCfg"></param>
        /// <param name="active"></param>
        void SetSkillActive(SkillCfg skillCfg, bool active);

        List<CastCfg> GetActiveCastCfgs();

        void UpdateCastConfig(ICastInput input);

        List<ISkill> GetActiveSkills();

        ISkill GetActiveSkill();

        /// <summary>
        /// 施法者本身的actor
        /// </summary>
        /// <returns></returns>
        IActor GetActor();

        CastMsg BuffAllowCast(ISkill skill);

        CastMsg ConsumeEnough(ISkill skill);

        CastMsg CdEnough(ISkill skill);

        Transform GetCastTransform(string name);
    }
}