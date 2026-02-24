import { defineStore } from 'pinia';

import { useGameLogStore } from './gameLog';
import { useLocationStore } from './location';
import { useUserStore } from './user';

export const useConnectProtocolStore = defineStore('ConnectProtocol', () => {
    const gameLogStore = useGameLogStore();
    const locationStore = useLocationStore();
    const userStore = useUserStore();

    /**
     * @param {number} count
     */
    function onOnlineFriendCountUpdated(count) {
        window.ConnectProtocol.SendEventAsync(
            'online-friend-count-updated',
            JSON.stringify({
                newOnlineFriendCount: count
            })
        );
    }

    /**
     * @param {any[]} feedData
     */
    function onFeedUpdated(feedData) {
        window.ConnectProtocol.SendEventAsync(
            'feed-updated',
            JSON.stringify(feedData)
        );
    }

    function onLocationUpdated() {
        const lastLocation = {
            date: locationStore.lastLocation.date,
            location: locationStore.lastLocation.location,
            name: locationStore.lastLocation.name,
            players: Array.from(locationStore.lastLocation.playerList.values()),
            friends: Array.from(locationStore.lastLocation.friendList.values()),
            onlineFor: userStore.currentUser.$online_for
        };

        window.ConnectProtocol.SendEventAsync(
            'location-updated',
            JSON.stringify(lastLocation)
        );
    }

    function onMediaUpdated() {
        window.ConnectProtocol.SendEventAsync(
            'in-game-media-status-updated',
            JSON.stringify({
                urlOrName: gameLogStore.nowPlaying.url,
                loadRequestedAt: gameLogStore.nowPlaying.startTime,
                richMediaSupportedAndPlaying: gameLogStore.nowPlaying.playing,
                richMediaName: gameLogStore.nowPlaying.name,
                richMediaLength: gameLogStore.nowPlaying.length,
                richMediaElapsed: gameLogStore.nowPlaying.elapsed,
                richMediaThumbnailUrl: gameLogStore.nowPlaying.thumbnailUrl
            })
        );
    }

    /**
     * @param {object} noty
     * @param {string?} message
     * @param {string?} pathToLocalImage
     */
    function sendNotification(noty, message, pathToLocalImage) {
        window.ConnectProtocol.SendEventAsync(
            'internal-noty-notification',
            JSON.stringify({
                noty,
                message,
                imageLocalPath: pathToLocalImage
            })
        );
    }

    return {
        onOnlineFriendCountUpdated,
        onFeedUpdated,
        onMediaUpdated,
        onLocationUpdated,
        sendNotification
    };
});
