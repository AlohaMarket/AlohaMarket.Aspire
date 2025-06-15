global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;

global using Aloha.EventBus;
global using Aloha.EventBus.Events;
global using Aloha.EventBus.Models;
global using Aloha.MicroService.Post.Apis;
global using Aloha.MicroService.Post.Bootstraping;
global using Aloha.ServiceDefaults.DependencyInjection;
global using Aloha.ServiceDefaults.Hosting;
global using Aloha.ServiceDefaults.Middlewares;

global using MediatR;

global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Http.HttpResults;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;