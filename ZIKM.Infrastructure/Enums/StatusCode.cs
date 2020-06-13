namespace ZIKM.Infrastructure.Enums {
    public enum StatusCode : short {
        /// <summary>
        /// Internal server error
        /// </summary>
        ServerError = -3,
        /// <summary>
        /// Session forcibly ended
        /// </summary>
        SessionLost = -2,
        /// <summary>
        /// Bad request
        /// </summary>
        BadRequest = -1,
        /// <summary>
        /// Successfully
        /// </summary>
        Success = 0,
        /// <summary>
        /// Bad client data or action
        /// </summary>
        BadData = 1,
        /// <summary>
        /// User doesn't have access to object
        /// </summary>
        NoAccess = 2,
        /// <summary>
        /// User has blocked
        /// </summary>
        Blocked = 3
    }
}
