[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)]
  [String]$BlobUrl,

  [Parameter(Mandatory=$true)]
  [String]$SaSToken,

  [Parameter(Mandatory=$true)]
  [String]$Destination

)

Write-Host "Downloading blob: $BlobUrl"


$blobUrlSaS = $BlobUrl + $SaSToken

.\azcopy copy $blobUrlSas $Destination