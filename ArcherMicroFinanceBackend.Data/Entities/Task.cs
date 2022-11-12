﻿using NukesLab.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PanoramBackend.Data.Entities
{
    public class TaskTodo:BaseEntity<int>
    {
        public string TaskName { get; set; }
        public int? AssignedToId { get; set; }
        public int? AssignedById { get; set; }
        public DateTime? DueDate { get; set; }
        //public DateTime Time { get; set; }
        public string Time { get; set; }
        public int? PriorityId { get; set; }
        public int? StatusId { get; set; }
        public string Notes { get; set; }
        public bool SendUpdate { get; set; }
        public virtual UserDetails AssignedTo { get; set; }
        public virtual UserDetails AssignedBy { get; set; }
        public virtual Status Status { get; set; }
        public virtual Priority Priority { get; set; }
    }
    public class Status : BaseEntity<int>
    {
       
        public string Name { get; set; }
        private ICollection<TaskTodo> _task;
        public virtual ICollection<TaskTodo> TaskTodos => _task??(_task = new List<TaskTodo>());
    }
    public class Priority : BaseEntity<int>
    {
        public string Name { get; set; }
        private ICollection<TaskTodo> _task;
        public virtual ICollection<TaskTodo> TaskTodos => _task??(_task = new List<TaskTodo>());
    }
}
