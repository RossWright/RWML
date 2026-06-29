using System.ComponentModel.DataAnnotations;
using RossWright.MetalChain;
using RossWright.MetalNexus;

namespace RossWright.MetalNexus.Integration.Tests.Infrastructure;

// ── Phase A ──────────────────────────────────────────────────────────────────

/// <summary>Simple echo request sent as a GET with query parameters.</summary>
[Anonymous]
[ApiRequest(path: "/api/integration-tests/echo")]
public class EchoRequest : IRequest<EchoResponse>
{
    public string Message { get; set; } = string.Empty;
}

public class EchoResponse
{
    public string Echo { get; set; } = string.Empty;
}

// ── Phase B — Query Params ────────────────────────────────────────────────────

public enum TestColor { Red, Green, Blue }

/// <summary>Proves string, int, and enum query params arrive in the handler.</summary>
/// <remarks>
/// bool is intentionally excluded: BuildJsonObjectFromQuery emits string nodes for query
/// values, and STJ cannot coerce a JSON string to bool without AllowReadingFromString on
/// booleans.  The bool-from-query-param scenario is already covered by the unit tests
/// for BuildJsonObjectFromQuery.
/// </remarks>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/simple-props")]
public class SimplePropsRequest : IRequest<SimplePropsResponse>
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public TestColor Color { get; set; }
}

public class SimplePropsResponse
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public TestColor Color { get; set; }
}

/// <summary>Proves a repeated query-key array arrives as a populated string array.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/array-prop")]
public class ArrayPropRequest : IRequest<ArrayPropResponse>
{
    public string[] Tags { get; set; } = [];
}

public class ArrayPropResponse
{
    public string[] Tags { get; set; } = [];
}

/// <summary>
/// Proves a nested complex-type property inside a JSON body is deserialized correctly.
/// The registry rejects Get/PostViaQuery for requests with complex properties, so this
/// intentionally uses PostViaBody — the unit tests already cover BuildJsonObjectFromQuery
/// for nested types directly.
/// </summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaBody, path: "/api/integration-tests/nested")]
public class NestedRequest : IRequest<NestedResponse>
{
    public NestedInner Inner { get; set; } = new();
}

public class NestedInner
{
    public string Value { get; set; } = string.Empty;
}

public class NestedResponse
{
    public string InnerValue { get; set; } = string.Empty;
}

// ── Phase B — JSON Body ───────────────────────────────────────────────────────

/// <summary>Proves a POST with a JSON body reaches the handler with all properties set.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaBody, path: "/api/integration-tests/json-body")]
public class JsonBodyRequest : IRequest<JsonBodyResponse>
{
    public string Title { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class JsonBodyResponse
{
    public string Title { get; set; } = string.Empty;
    public int Value { get; set; }
}

// ── Phase B — Path Params ─────────────────────────────────────────────────────

/// <summary>Proves a single path slot `/api/…/{Id}` is hydrated into the int property.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/path-single/{Id}")]
public class PathSingleRequest : IRequest<PathSingleResponse>
{
    public int Id { get; set; }
}

public class PathSingleResponse
{
    public int Id { get; set; }
}

/// <summary>Proves two path slots are each hydrated into the correct properties.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/path-multi/{UserId}/items/{ItemId}")]
public class PathMultiRequest : IRequest<PathMultiResponse>
{
    public int UserId { get; set; }
    public int ItemId { get; set; }
}

public class PathMultiResponse
{
    public int UserId { get; set; }
    public int ItemId { get; set; }
}

// ── Phase B — FromHeader ──────────────────────────────────────────────────────

/// <summary>Proves a [FromHeader] property is populated from the named HTTP header.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/from-header")]
public class FromHeaderRequest : IRequest<FromHeaderResponse>
{
    [FromHeader("X-Test-Token")]
    public string Token { get; set; } = string.Empty;
}

public class FromHeaderResponse
{
    public string Token { get; set; } = string.Empty;
}

// ── Phase B — Raw Request Body ────────────────────────────────────────────────

/// <summary>Proves the raw request body arrives verbatim via IMetalNexusRawRequest.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaBody, path: "/api/integration-tests/raw-body")]
public class RawBodyRequest : IMetalNexusRawRequest<RawBodyResponse>
{
    public string? RawRequestBody { get; set; }
}

public class RawBodyResponse
{
    public string? Body { get; set; }
}

// ── Phase D — Exception / Error Mapping ──────────────────────────────────────

/// <summary>Proves that a MetalNexusException thrown by a handler maps to 400 on the client.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/throw-metalNexus")]
public class ThrowMetalNexusRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that an InternalServerErrorException maps to 500 on the client.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/throw-internal-server-error")]
public class ThrowInternalServerErrorRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that a ValidationException maps to 422 on the client.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/throw-validation")]
public class ThrowValidationRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that a NotAuthenticatedException maps to 401 on the client.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/throw-unauthorized")]
public class ThrowUnauthorizedRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves the IncludeServerStackTraceOnExceptions option controls StackTrace presence.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/throw-for-stack-trace")]
public class ThrowForStackTraceRequest : IRequest<EmptyResponse>
{
}

public class EmptyResponse { }

// ── Phase E — Authentication & Authorization ─────────────────────────────────

/// <summary>Proves that an [Anonymous] endpoint is accessible without credentials.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/auth/anonymous")]
public class AnonymousEndpointRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that an [Authenticated] endpoint rejects unauthenticated callers.</summary>
[Authenticated]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/auth/require-auth")]
public class AuthenticatedEndpointRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that a provisional token is rejected on an endpoint that does not allow it.</summary>
[Authenticated]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/auth/no-provisional")]
public class ProvisionalDisallowedRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that a provisional token is accepted on an endpoint that sets AllowProvisional.</summary>
[Authenticated(AllowProvisional = true)]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/auth/allow-provisional")]
public class ProvisionalAllowedRequest : IRequest<EmptyResponse>
{
}

/// <summary>Proves that a user with the wrong role is rejected with 403.</summary>
[Authenticated("Admin")]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/auth/require-admin")]
public class AdminRoleRequest : IRequest<EmptyResponse>
{
}

// ── Phase C — Response Serialization ─────────────────────────────────────────

/// <summary>Proves that a void handler (IRequest, no response type) returns 200 with no body.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaBody, path: "/api/integration-tests/void-response")]
public class VoidResponseRequest : IRequest
{
    public string Value { get; set; } = string.Empty;
}

/// <summary>Proves that a typed response round-trips through JSON correctly.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/json-response")]
public class JsonResponseRequest : IRequest<JsonResponseResponse>
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class JsonResponseResponse
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public string Computed { get; set; } = string.Empty;
}

/// <summary>Proves that a MetalNexusFile response is downloaded correctly on the client.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/file-download")]
public class FileDownloadRequest : IRequest<MetalNexusFile>
{
    public string FileName { get; set; } = "test.txt";
}

/// <summary>Proves that IsAttachment=false produces a Content-Disposition: inline header.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/file-download-inline")]
public class FileDownloadInlineRequest : IRequest<MetalNexusFile>
{
}

// ── Phase B — File Uploads ────────────────────────────────────────────────────

/// <summary>Proves a single multipart upload arrives in Files[0].</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/file-single")]
public class FileSingleRequest : MetalNexusFileRequest, IRequest<FileSingleResponse>
{
}

public class FileSingleResponse
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int ByteCount { get; set; }
}

/// <summary>Proves multiple files in one multipart request all appear in Files[].</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/file-multiple")]
public class FileMultipleRequest : MetalNexusFileRequest, IRequest<FileMultipleResponse>
{
}

public class FileMultipleResponse
{
    public int FileCount { get; set; }
    public string[] FileNames { get; set; } = [];
}

/// <summary>Proves a file sent with a [FileSlot] name routes to the typed property, not Files[].</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/file-named-slot")]
public class FileNamedSlotRequest : MetalNexusFileRequest, IRequest<FileNamedSlotResponse>
{
    [FileSlot("avatar")]
    public MetalNexusFile? Avatar { get; set; }
}

public class FileNamedSlotResponse
{
    public string? SlotFileName { get; set; }
    public int AnonymousFileCount { get; set; }
}

/// <summary>Proves a file with no matching slot name lands in Files[], not a property.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/file-unmatched-slot")]
public class FileUnmatchedSlotRequest : MetalNexusFileRequest, IRequest<FileUnmatchedSlotResponse>
{
    [FileSlot("avatar")]
    public MetalNexusFile? Avatar { get; set; }
}

public class FileUnmatchedSlotResponse
{
    public string? SlotFileName { get; set; }
    public int AnonymousFileCount { get; set; }
}

/// <summary>Proves two [FileSlot] properties receive distinct files by form-field name.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/file-two-slots")]
public class FileTwoSlotsRequest : MetalNexusFileRequest, IRequest<FileTwoSlotsResponse>
{
    [FileSlot("front")]
    public MetalNexusFile? Front { get; set; }

    [FileSlot("back")]
    public MetalNexusFile? Back { get; set; }
}

public class FileTwoSlotsResponse
{
    public string? FrontFileName { get; set; }
    public string? BackFileName { get; set; }
    public int AnonymousFileCount { get; set; }
}

// ── Phase F — Routing Edge Cases ──────────────────────────────────────────────

/// <summary>
/// Static route that shares a path prefix with <see cref="RoutingParamRequest"/>.
/// The middleware must prefer this exact match over the bracket pattern.
/// </summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/routing/static")]
public class RoutingStaticRequest : IRequest<RoutingResponse>
{
}

/// <summary>
/// Path-param route — should only be matched when no static route takes priority.
/// </summary>
[Anonymous]
[ApiRequest(HttpProtocol.Get, path: "/api/integration-tests/routing/{Token}")]
public class RoutingParamRequest : IRequest<RoutingResponse>
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>Response shared by routing test endpoints so tests can distinguish which handler ran.</summary>
public class RoutingResponse
{
    public string Handler { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

/// <summary>POST-only endpoint — a GET to this path must fall through to the next middleware.</summary>
[Anonymous]
[ApiRequest(HttpProtocol.PostViaBody, path: "/api/integration-tests/routing-post-only")]
public class PostOnlyRequest : IRequest<RoutingResponse>
{
}

// ── Phase G — File Validation Attributes ─────────────────────────────────────

/// <summary>Proves class-level [MaxFileSize] rejects an oversized file in Files[].</summary>
[Anonymous]
[MaxFileSize(10)]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g2/max-size-class")]
public class MaxSizeClassRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
}

/// <summary>Proves class-level [AllowedFileTypes] rejects a disallowed MIME type in Files[].</summary>
[Anonymous]
[AllowedFileTypes("image/png")]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g2/allowed-types-class")]
public class AllowedTypesClassRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
}

/// <summary>Proves class-level [MaxFileCount] rejects too many files.</summary>
[Anonymous]
[MaxFileCount(1)]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g2/max-count-class")]
public class MaxCountClassRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
}

/// <summary>
/// Has a class-level [MaxFileSize] of 10 bytes but a slot-level override of 100 bytes.
/// Proves property-level overrides the class default for the named slot.
/// </summary>
[Anonymous]
[MaxFileSize(10)]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g2/max-size-prop-override")]
public class MaxSizePropOverrideRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
    [FileSlot("big")]
    [MaxFileSize(100)]
    public MetalNexusFile? BigFile { get; set; }
}

/// <summary>
/// Has a class-level [AllowedFileTypes] of image/png but a slot-level override allowing text/plain.
/// Proves property-level list is used for that slot, ignoring class default.
/// </summary>
[Anonymous]
[AllowedFileTypes("image/png")]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g2/allowed-types-prop-override")]
public class AllowedTypesPropOverrideRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
    [FileSlot("doc")]
    [AllowedFileTypes("text/plain")]
    public MetalNexusFile? Document { get; set; }
}

// ── Phase G3 — Upload Limit Null Hardening ────────────────────────────────────

/// <summary>Proves [UploadLimit] does not throw when IHttpMaxRequestBodySizeFeature is null.</summary>
[Anonymous]
[UploadLimit(1_000_000)]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g3/upload-limit")]
public class UploadLimitRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
}

/// <summary>Proves [NoUploadLimit] does not throw when IHttpMaxRequestBodySizeFeature is null.</summary>
[Anonymous]
[NoUploadLimit]
[ApiRequest(HttpProtocol.PostViaQuery, path: "/api/integration-tests/g3/no-upload-limit")]
public class NoUploadLimitRequest : MetalNexusFileRequest, IRequest<EmptyResponse>
{
}
