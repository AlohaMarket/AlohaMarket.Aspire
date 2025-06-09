global using System;
global using System.Text;
global using System.Text.Json;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Configuration;

global using Microsoft.EntityFrameworkCore;

global using MediatR;

global using Aloha.EventBus;
global using Aloha.EventBus.Events;
global using Aloha.EventBus.Models;
