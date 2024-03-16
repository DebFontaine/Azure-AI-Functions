using API.DTOs;

namespace API.Dtos;
public class DriveDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ColorRgb { get; set; }
    public string Kind { get; set; }
    public string BackgroundImageLink { get; set; }
    public Capabilities Capabilities { get; set; }
    public string ThemeId { get; set; }
    public BackgroundImageFile BackgroundImageFile { get; set; }
    public string CreatedTime { get; set; }
    public bool Hidden { get; set; }
    public Restrictions Restrictions { get; set; }
    public string OrgUnitId { get; set; }
}

public class Capabilities
{
    public bool CanAddChildren { get; set; }
    public bool CanComment { get; set; }
    public bool CanCopy { get; set; }
    public bool CanDeleteDrive { get; set; }
    public bool CanDownload { get; set; }
    public bool CanEdit { get; set; }
    public bool CanListChildren { get; set; }
    public bool CanManageMembers { get; set; }
    public bool CanReadRevisions { get; set; }
    public bool CanRename { get; set; }
    public bool CanRenameDrive { get; set; }
    public bool CanChangeDriveBackground { get; set; }
    public bool CanShare { get; set; }
    public bool CanChangeCopyRequiresWriterPermissionRestriction { get; set; }
    public bool CanChangeDomainUsersOnlyRestriction { get; set; }
    public bool CanChangeDriveMembersOnlyRestriction { get; set; }
    public bool CanChangeSharingFoldersRequiresOrganizerPermissionRestriction { get; set; }
    public bool CanResetDriveRestrictions { get; set; }
    public bool CanDeleteChildren { get; set; }
    public bool CanTrashChildren { get; set; }
}

public class BackgroundImageFile
{
    public string Id { get; set; }
    public double XCoordinate { get; set; }
    public double YCoordinate { get; set; }
    public double Width { get; set; }
}

public class Restrictions
{
    public bool CopyRequiresWriterPermission { get; set; }
    public bool DomainUsersOnly { get; set; }
    public bool DriveMembersOnly { get; set; }
    public bool AdminManagedRestrictions { get; set; }
    public bool SharingFoldersRequiresOrganizerPermission { get; set; }
}