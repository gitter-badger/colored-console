﻿// <copyright file="BauPack.cs" company="Bau contributors">
//  Copyright (c) Bau contributors. (baubuildch@gmail.com)
// </copyright>

namespace Bau
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using ScriptCs.Contracts;

    public class BauPack : IScriptPackContext, ITaskBuilder
    {
        public const string DefaultTask = "default";

        private readonly List<string> topLevelTasks = new List<string>();
        private readonly Dictionary<string, Task> tasks = new Dictionary<string, Task>();
        private Task currentTask;

        // TODO (adamralph): change to params and default to default task, take responsibility out of BauScriptPack
        public BauPack(params string[] topLevelTasks)
        {
            this.topLevelTasks.AddRange(topLevelTasks);
            if (this.topLevelTasks.Count == 0)
            {
                this.topLevelTasks.Add(BauPack.DefaultTask);
            }
        }

        public Task CurrentTask
        {
            get { return this.currentTask; }
        }

        public ITaskBuilder DependsOn(params string[] otherTasks)
        {
            this.EnsureCurrentTask();
            foreach (var task in otherTasks.Where(t => !this.currentTask.Dependencies.Contains(t)))
            {
                if (string.IsNullOrWhiteSpace(task))
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "Invalid task name '{0}'.", task);
                    throw new ArgumentException(message, "otherTasks");
                }

                this.currentTask.Dependencies.Add(task);
            }

            return this;
        }

        public ITaskBuilder Do(Action action)
        {
            this.EnsureCurrentTask();
            if (action != null)
            {
                this.currentTask.Actions.Add(action);
            }

            return this;
        }

        public void Execute()
        {
            var version = (AssemblyInformationalVersionAttribute)Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute)).Single();

            Console.WriteLine("Bau version {0}.", version.InformationalVersion);
            Console.WriteLine("Copyright (c) Bau contributors. (baubuildch@gmail.com)");

            foreach (var task in this.topLevelTasks.Select(name => this.GetTask(name)))
            {
                task.Invoke(this);
            }

            Console.WriteLine("Bau succeeded.");
        }

        public Task GetTask(string name)
        {
            Task task;
            if (!this.tasks.TryGetValue(name, out task))
            {
                var message = string.Format(CultureInfo.InvariantCulture, "'{0}' task not found.", name);
                throw new InvalidOperationException(message);
            }

            return task;
        }

        public ITaskBuilder Intern<TTask>(string name = BauPack.DefaultTask) where TTask : Task, new()
        {
            Task task;
            if (!this.tasks.TryGetValue(name, out task))
            {
                this.tasks.Add(name, task = new TTask() { Name = name });
            }

            var typedTask = task as TTask;
            if (typedTask == null)
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "'{0}' task already exists with type '{1}'.",
                    name,
                    task.GetType().Name);

                throw new InvalidOperationException(message);
            }

            this.currentTask = typedTask;
            return this;
        }

        private void EnsureCurrentTask()
        {
            if (this.currentTask == null)
            {
                this.Intern<Task>(BauPack.DefaultTask);
            }
        }
    }
}
