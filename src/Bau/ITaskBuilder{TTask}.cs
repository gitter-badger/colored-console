﻿// <copyright file="ITaskBuilder{TTask}.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace Bau
{
    using System;

    public interface ITaskBuilder<TTask> where TTask : Task, new()
    {
        ITaskBuilder<TTask> Intern(string name = BauPack.DefaultTask);

        ITaskBuilder<TTask> DependsOn(params string[] otherTasks);

        ITaskBuilder<TTask> Do(Action<TTask> action);

        void Execute();
    }
}
