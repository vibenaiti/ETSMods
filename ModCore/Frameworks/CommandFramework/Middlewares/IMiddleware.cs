using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ModCore;
using ModCore.Events;
using ModCore.Models;
using static ModCore.Frameworks.CommandFramework.CommandFramework;

public interface IMiddleware
{
	public bool CanExecute(Player sender, CommandAttribute command, MethodInfo method);
}
