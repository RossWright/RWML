using System.ComponentModel.DataAnnotations;
using RossWright.MetalChain;

namespace RossWright.MetalNexus.Integration.Tests.Infrastructure;

// ── Phase A ───────────────────────────────────────────────────────────────────

// ── Phase F ───────────────────────────────────────────────────────────────────

internal class RoutingStaticHandler : IRequestHandler<RoutingStaticRequest, RoutingResponse>
{
    public Task<RoutingResponse> Handle(RoutingStaticRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new RoutingResponse { Handler = "static" });
}

internal class RoutingParamHandler : IRequestHandler<RoutingParamRequest, RoutingResponse>
{
    public Task<RoutingResponse> Handle(RoutingParamRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new RoutingResponse { Handler = "param", Token = request.Token });
}

internal class PostOnlyHandler : IRequestHandler<PostOnlyRequest, RoutingResponse>
{
    public Task<RoutingResponse> Handle(PostOnlyRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new RoutingResponse { Handler = "post-only" });
}

// ── Phase A ───────────────────────────────────────────────────────────────────

internal class EchoHandler : IRequestHandler<EchoRequest, EchoResponse>
{
    public Task<EchoResponse> Handle(EchoRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EchoResponse { Echo = request.Message });
}

// ── Phase D ───────────────────────────────────────────────────────────────────

// ── Phase E ───────────────────────────────────────────────────────────────────

internal class AnonymousEndpointHandler : IRequestHandler<AnonymousEndpointRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(AnonymousEndpointRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class AuthenticatedEndpointHandler : IRequestHandler<AuthenticatedEndpointRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(AuthenticatedEndpointRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class ProvisionalDisallowedHandler : IRequestHandler<ProvisionalDisallowedRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ProvisionalDisallowedRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class ProvisionalAllowedHandler : IRequestHandler<ProvisionalAllowedRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ProvisionalAllowedRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class AdminRoleHandler : IRequestHandler<AdminRoleRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(AdminRoleRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

// ── Phase D ───────────────────────────────────────────────────────────────────

internal class ThrowMetalNexusHandler : IRequestHandler<ThrowMetalNexusRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ThrowMetalNexusRequest request, CancellationToken cancellationToken) =>
        throw new MetalNexusException("test bad-request error");
}

internal class ThrowInternalServerErrorHandler : IRequestHandler<ThrowInternalServerErrorRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ThrowInternalServerErrorRequest request, CancellationToken cancellationToken) =>
        throw new InternalServerErrorException("test internal server error");
}

internal class ThrowValidationHandler : IRequestHandler<ThrowValidationRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ThrowValidationRequest request, CancellationToken cancellationToken) =>
        throw new ValidationException("test validation error");
}

internal class ThrowUnauthorizedHandler : IRequestHandler<ThrowUnauthorizedRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ThrowUnauthorizedRequest request, CancellationToken cancellationToken) =>
        throw new NotAuthenticatedException("test unauthorized error");
}

internal class ThrowForStackTraceHandler : IRequestHandler<ThrowForStackTraceRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(ThrowForStackTraceRequest request, CancellationToken cancellationToken) =>
        throw new MetalNexusException("test stack trace error");
}

// ── Phase C ───────────────────────────────────────────────────────────────────

internal class VoidResponseHandler : IRequestHandler<VoidResponseRequest>
{
    public Task Handle(VoidResponseRequest request, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

internal class JsonResponseHandler : IRequestHandler<JsonResponseRequest, JsonResponseResponse>
{
    public Task<JsonResponseResponse> Handle(JsonResponseRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new JsonResponseResponse
        {
            Name = request.Name,
            Count = request.Count,
            Computed = $"{request.Name}:{request.Count}",
        });
}

internal class FileDownloadHandler : IRequestHandler<FileDownloadRequest, MetalNexusFile>
{
    public Task<MetalNexusFile> Handle(FileDownloadRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new MetalNexusFile
        {
            FileName = request.FileName,
            ContentType = "text/plain",
            Data = System.Text.Encoding.UTF8.GetBytes($"Hello from {request.FileName}"),
            IsAttachment = true,
        });
}

internal class FileDownloadInlineHandler : IRequestHandler<FileDownloadInlineRequest, MetalNexusFile>
{
    public Task<MetalNexusFile> Handle(FileDownloadInlineRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new MetalNexusFile
        {
            FileName = "inline.txt",
            ContentType = "text/plain",
            Data = System.Text.Encoding.UTF8.GetBytes("inline content"),
            IsAttachment = false,
        });
}

// ── Phase B ───────────────────────────────────────────────────────────────────

internal class SimplePropsHandler : IRequestHandler<SimplePropsRequest, SimplePropsResponse>
{
    public Task<SimplePropsResponse> Handle(SimplePropsRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new SimplePropsResponse
        {
            Name = request.Name,
            Count = request.Count,
            Color = request.Color,
        });
}

internal class ArrayPropHandler : IRequestHandler<ArrayPropRequest, ArrayPropResponse>
{
    public Task<ArrayPropResponse> Handle(ArrayPropRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new ArrayPropResponse { Tags = request.Tags });
}

internal class NestedHandler : IRequestHandler<NestedRequest, NestedResponse>
{
    public Task<NestedResponse> Handle(NestedRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new NestedResponse { InnerValue = request.Inner.Value });
}

internal class JsonBodyHandler : IRequestHandler<JsonBodyRequest, JsonBodyResponse>
{
    public Task<JsonBodyResponse> Handle(JsonBodyRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new JsonBodyResponse { Title = request.Title, Value = request.Value });
}

internal class PathSingleHandler : IRequestHandler<PathSingleRequest, PathSingleResponse>
{
    public Task<PathSingleResponse> Handle(PathSingleRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new PathSingleResponse { Id = request.Id });
}

internal class PathMultiHandler : IRequestHandler<PathMultiRequest, PathMultiResponse>
{
    public Task<PathMultiResponse> Handle(PathMultiRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new PathMultiResponse { UserId = request.UserId, ItemId = request.ItemId });
}

internal class FromHeaderHandler : IRequestHandler<FromHeaderRequest, FromHeaderResponse>
{
    public Task<FromHeaderResponse> Handle(FromHeaderRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new FromHeaderResponse { Token = request.Token });
}

internal class RawBodyHandler : IRequestHandler<RawBodyRequest, RawBodyResponse>
{
    public Task<RawBodyResponse> Handle(RawBodyRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new RawBodyResponse { Body = request.RawRequestBody });
}

internal class FileSingleHandler : IRequestHandler<FileSingleRequest, FileSingleResponse>
{
    public async Task<FileSingleResponse> Handle(FileSingleRequest request, CancellationToken cancellationToken)
    {
        var file = request.Files[0];
        int byteCount = 0;
        if (file.DataStream != null)
        {
            using var ms = new MemoryStream();
            await file.DataStream.CopyToAsync(ms, cancellationToken);
            byteCount = (int)ms.Length;
        }
        return new FileSingleResponse
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            ByteCount = byteCount,
        };
    }
}

internal class FileMultipleHandler : IRequestHandler<FileMultipleRequest, FileMultipleResponse>
{
    public Task<FileMultipleResponse> Handle(FileMultipleRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new FileMultipleResponse
        {
            FileCount = request.Files.Length,
            FileNames = request.Files.Select(f => f.FileName).ToArray(),
        });
}

internal class FileNamedSlotHandler : IRequestHandler<FileNamedSlotRequest, FileNamedSlotResponse>
{
    public Task<FileNamedSlotResponse> Handle(FileNamedSlotRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new FileNamedSlotResponse
        {
            SlotFileName = request.Avatar?.FileName,
            AnonymousFileCount = request.Files?.Length ?? 0,
        });
}

internal class FileUnmatchedSlotHandler : IRequestHandler<FileUnmatchedSlotRequest, FileUnmatchedSlotResponse>
{
    public Task<FileUnmatchedSlotResponse> Handle(FileUnmatchedSlotRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new FileUnmatchedSlotResponse
        {
            SlotFileName = request.Avatar?.FileName,
            AnonymousFileCount = request.Files?.Length ?? 0,
        });
}

internal class FileTwoSlotsHandler : IRequestHandler<FileTwoSlotsRequest, FileTwoSlotsResponse>
{
    public Task<FileTwoSlotsResponse> Handle(FileTwoSlotsRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new FileTwoSlotsResponse
        {
            FrontFileName = request.Front?.FileName,
            BackFileName = request.Back?.FileName,
            AnonymousFileCount = request.Files?.Length ?? 0,
        });
}

// ── Phase G — File Validation & Upload Limit ──────────────────────────────────

internal class MaxSizeClassHandler : IRequestHandler<MaxSizeClassRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(MaxSizeClassRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class AllowedTypesClassHandler : IRequestHandler<AllowedTypesClassRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(AllowedTypesClassRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class MaxCountClassHandler : IRequestHandler<MaxCountClassRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(MaxCountClassRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class MaxSizePropOverrideHandler : IRequestHandler<MaxSizePropOverrideRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(MaxSizePropOverrideRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class AllowedTypesPropOverrideHandler : IRequestHandler<AllowedTypesPropOverrideRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(AllowedTypesPropOverrideRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class UploadLimitHandler : IRequestHandler<UploadLimitRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(UploadLimitRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}

internal class NoUploadLimitHandler : IRequestHandler<NoUploadLimitRequest, EmptyResponse>
{
    public Task<EmptyResponse> Handle(NoUploadLimitRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new EmptyResponse());
}
