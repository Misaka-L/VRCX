# Events

This document describe all event types in VRCX Connect SSE.
For details about SSE in VRCX Connect, see [SSE README](README.md).

> [!CAUTION]
> Any field with nullable type (e.g `string?`) can be garbage value (e.g `0`, `null` in `int?`, or empty string, `null` in `string?`)
>
> Check field Description and Event Alerts (e.g CAUTION) for more information.

> [!CAUTION]
> DO NOT use any field didn't show up in this document.
> We are not Windows, we won't promise compatibility.

## `hello`

A empty json object send when SSE connection established.

```json
{}
```

## `user-tag-update`

| Field      | Type      | Description      | Example                                    |
| ---------- | --------- | ---------------- | ------------------------------------------ |
| `userId`   | `string`  | VRChat User Id   | `usr_c6806bef-b885-427d-acaf-e24a66acb829` |
| `tagColor` | `string?` | Tag Color in Hex | `#ffffff`                                  |

```json
{
    "userId": "usr_c6806bef-b885-427d-acaf-e24a66acb829",
    "tagColor": "#ffffff"
}
```

## `online-friend-count-updated`

| Field                  | Type  | Description                       | Example |
| ---------------------- | ----- | --------------------------------- | ------- |
| `newOnlineFriendCount` | `int` | Online friend count after updated | `24`    |

```json
{
    "newOnlineFriendCount": 24
}
```

## `internal-noty-notification`

| Field            | Type      | Description                              | Example                       |
| ---------------- | --------- | ---------------------------------------- | ----------------------------- |
| `noty`           | `string?` | VRCX internal noty object in JSON string | See below                     |
| `message`        | `string?` | Message of notification                  | N/A                           |
| `imageLocalPath` | `string?` | Path to local image for notification     | `C:\images\notification.jpeg` |

```json
{
    "message": "",
    "noty": "{\"id\":\"not_3fe41b16-7ab6-4241-b60c-4457e065442c\",\"senderUserId\":\"usr_2891d324-252f-4829-8036-039017430757\",\"senderUsername\":\"_Eo_\",\"type\":\"requestInvite\",\"message\":\"\",\"details\":{},\"seen\":false,\"created_at\":\"2026-02-23T10:28:13.632Z\",\"$isExpired\":false,\"receiverUserId\":\"usr_c6806bef-b885-427d-acaf-e24a66acb829\",\"isFriend\":true,\"isFavorite\":false}",
    "imageLocalPath": "C:\\images\\notification.jpeg"
}
```

### `noty` Example

> [!CAUTION]
> `noty` object are internal implementation of VRCX. It's a rabbit hole that nobody figure out how it works.

```json
{
    "id": "not_3fe41b16-7ab6-4241-b60c-4457e065442c",
    "senderUserId": "usr_2891d324-252f-4829-8036-039017430757",
    "senderUsername": "_Eo_",
    "type": "requestInvite",
    "message": "",
    "details": {},
    "seen": false,
    "created_at": "2026-02-23T10:28:13.632Z",
    "$isExpired": false,
    "receiverUserId": "usr_c6806bef-b885-427d-acaf-e24a66acb829",
    "isFriend": true,
    "isFavorite": false
}
```

## `in-game-media-status-updated`

> [!IMPORTANT]
> Only [PyPyDance](https://vrchat.com/home/world/wrld_f20326da-f1ac-45fc-a062-609723b097b1) and [Popcorn Palace](https://vrchat.com/home/world/wrld_266523e8-9161-40da-acd0-6bd82e075833) support Rich Media Status

> [!CAUTION]
> DO NOT use any rich media status information when `rich-media-supported-and-playing` is `false`.

| Field                          | Type      | Description                                                                                         | Example                                                                                                   |
| ------------------------------ | --------- | --------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| `urlOrName`                    | `string?` | Url to media playing or name of media, can be any url supported by VRChat (e.g Youtube, Url to mp4) | `https://www.youtube.com/watch?v=qnPGGqqGK3o` or `We Were Algorithms Once - Upload Labs Soundtrack (OST)` |
| `loadRequestedAt`              | `float?`  | Unix timestamp in seconds float                                                                     | `1771688566.942379`                                                                                       |
| `richMediaSupportedAndPlaying` | `bool`    | Is rich media supported and playing, see "Important" information above.                             | `true`                                                                                                    |
| `richMediaName`                | `string?` | Name of the media playing. (e.g In PyPyDance, it can be "Song name (Player who play the song)")     | `Le Freak (RandomPlayer)`                                                                                 |
| `richMediaLength`              | `float?`  | Media length in seconds                                                                             | `297`                                                                                                     |
| `richMediaElapsed`             | `float?`  | Elapsed time of media in seconds                                                                    | `2`                                                                                                       |
| `richMediaThumbnailUrl`        | `string?` | Url of thumbnailUrl                                                                                 | N/A                                                                                                       |

```json
{
    "url": "https://www.youtube.com/watch?v=qnPGGqqGK3o",
    "loadRequestedAt": 1771688566.942379,
    "richMediaSupportedAndPlaying": true,
    "richMediaName": "Le Freak (RandomPlayer)",
    "richMediaLength": 297,
    "richMediaElapsed": 2
}
```

## `feed-updated`

Array of feed store.

> [!CAUTION]
> Feed are internal implementation of VRCX. Some sample are given below but no promise provided.

```json
[
    {
        "created_at": "2026-02-21T15:49:33.000Z",
        "type": "Location",
        "location": "wrld_9a0938fb-c2b8-47c4-be27-f1a92fbd75d4:11992~hidden(usr_c6806bef-b885-427d-acaf-e24a66acb829)~region(jp)",
        "worldId": "wrld_9a0938fb-c2b8-47c4-be27-f1a92fbd75d4",
        "worldName": "AAAS Next - Demo",
        "groupName": "",
        "time": 0,
        "instanceDisplayName": "",
        "isFavorite": false,
        "isFriend": false,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:48:43.000Z",
        "type": "OnPlayerLeft",
        "displayName": "random-play",
        "location": "wrld_f20326da-f1ac-45fc-a062-609723b097b1:49762~friends(usr_c6806bef-b885-427d-acaf-e24a66acb829)~region(jp)",
        "userId": "usr_42366431-53c1-4343-a65c-373d84d43810",
        "time": 125000,
        "instanceDisplayName": "",
        "isFavorite": false,
        "isFriend": true,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:46:38.000Z",
        "type": "OnPlayerJoined",
        "displayName": "random-play",
        "location": "wrld_f20326da-f1ac-45fc-a062-609723b097b1:49762~friends(usr_c6806bef-b885-427d-acaf-e24a66acb829)~region(jp)",
        "userId": "usr_42366431-53c1-4343-a65c-373d84d43810",
        "time": 0,
        "instanceDisplayName": "",
        "isFavorite": false,
        "isFriend": true,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:42:45.000Z",
        "type": "Event",
        "data": "VideoError: [generic] Unable to download webpage: ('Connection aborted.', RemoteDisconnected('Remote end closed connection without response')) (caused by TransportError(\"('Connection aborted.', RemoteDisconnected('Remote end closed connection without response'))\"))",
        "isFavorite": false,
        "isFriend": false,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:30:17.898Z",
        "type": "GPS",
        "userId": "usr_3d3cfe28-2xfa-4f5g-37g4-3rfc2hxf2s2dd",
        "displayName": "edge-of",
        "location": "wrld_f40cdb86-9a1c-424c-9b9e-584233e04549:55863~hidden(usr_1bd5ex5c-dd76-4dre-ge0c-bz7b0e699231)~region(jp)",
        "worldName": "[CN] 简-BiliPlayer",
        "groupName": "",
        "previousLocation": "wrld_1c25919f-9d74-4984-8f83-99f0a81315ef:37411~hidden(usr_1bd5ex5c-dd76-4dre-ge0c-bz7b0e699231)~region(jp)",
        "time": 60057,
        "instanceDisplayName": "",
        "isFavorite": false,
        "isFriend": true,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:27:02.700Z",
        "type": "Status",
        "userId": "usr_edbf3red2-71d0-4cd4-a4ef-7r2fad4vdefe",
        "displayName": "nya-user",
        "status": "busy",
        "statusDescription": "Busy status description",
        "previousStatus": "busy",
        "previousStatusDescription": "sleeppppppppppppppppppp",
        "isFavorite": false,
        "isFriend": true,
        "tagColour": ""
    },
    {
        "created_at": "2026-02-21T15:42:42.147Z",
        "type": "Online",
        "userId": "usr_dybtx41d-c321-4rd6-xcdd-f2u7j4g4b5d4",
        "displayName": "Food-here",
        "location": "offline",
        "worldName": "",
        "groupName": "",
        "time": "",
        "instanceDisplayName": "",
        "isFavorite": false,
        "isFriend": true,
        "tagColour": ""
    }
]
```

## `location-update`

| Field       | Type                      | Description                                                 | Example                                                                                                       |
| ----------- | ------------------------- | ----------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| `date`      | `int?`                    | Unix timestamp of last location updated datetime in seconds | `1771688973000`                                                                                               |
| `location`  | `string?`                 | Location string of current location                         | `wrld_9a0938fb-c2b8-47c4-be27-f1a92fbd75d4:11992~hidden(usr_c6806bef-b885-427d-acaf-e24a66acb829)~region(jp)` |
| `name`      | `string?`                 | World Name                                                  | `VRChat Home World`                                                                                           |
| `players`   | `instance-user-object[]?` | Player in current instance                                  | N/A                                                                                                           |
| `friends`   | `instance-user-object[]?` | Friends in current instance                                 | N/A                                                                                                           |
| `onlineFor` | `int`                     | Current user online for                                     | `1771686642624`                                                                                               |

### Object `instance-user-object`

| Field         | Type     | Description                                       | Example                                    |
| ------------- | -------- | ------------------------------------------------- | ------------------------------------------ |
| `displayName` | `string` | Display name of the player                        | `VRCat`                                    |
| `userId`      | `string` | Id of the player                                  | `usr_c6806bef-b885-427d-acaf-e24a66acb829` |
| `joinAt`      | `int`    | Unix timestamp of when the player join in seconds | `1771688983000`                            |

```json
{
    "date": 1771688973000,
    "location": "wrld_9a0938fb-c2b8-47c4-be27-f1a92fbd75d4:11992~hidden(usr_c6806bef-b885-427d-acaf-e24a66acb829)~region(jp)",
    "name": "AAAS Next - Demo",
    "players": [
        {
            "displayName": "Misaka-L",
            "userId": "usr_c6806bef-b885-427d-acaf-e24a66acb829",
            "joinAt": 1771688983000
        }
    ],
    "friends": [
        {
            "displayName": "VRCat",
            "userId": "usr_sx3d6bff-bdc5-422d-ccff-edcw6sagbb2s",
            "joinAt": 1771688983000
        }
    ],
    "onlineFor": 1771686642624
}
```
