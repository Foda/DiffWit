﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffWit.ViewModel
{
    public interface IDiffViewModel
    {
        void ProcessDiff();

        int ChangeCount { get; }
    }
}
